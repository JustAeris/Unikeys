using System.Security.Cryptography;
using System.Text;

namespace Unikeys.Core.FileEncryption;

/// <summary>
/// Basic implementation of <see cref="Aes"/> encryption/decryption.
/// </summary>
public static class EncryptionDecryption
{
    private static readonly byte[] VersioningNumber = { Convert.ToByte(2) };

    /// <summary>
    /// Encrypt a file using AES-256-CBC. Supports any size. Includes a HMAC-SHA512 to ensure integrity.
    /// </summary>
    /// <param name="filePath">File to encrypt</param>
    /// <param name="destination">Destination file. Warning, it will overwrite any file with the same name</param>
    /// <param name="password">Optional, program will return a strong key if left empty</param>
    /// <param name="append">If true, will append the data to the destination file</param>
    /// <exception cref="FileNotFoundException">Specified file does not exist</exception>
    /// <exception cref="CryptographicException">The HMAC could not be calculated</exception>
    /// <returns>Used password, should only be used if the given password was empty</returns>
    public static string EncryptFile(string filePath, string destination, string password = "", bool append = false)
    {
        var key = string.IsNullOrEmpty(password) ? RandomNumberGenerator.GetBytes(32) : Encoding.UTF8.GetBytes(password);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

        using var aes = Aes.Create();
        aes.IV = RandomNumberGenerator.GetBytes(aes.IV.Length);
        aes.Key = Rfc2898DeriveBytes.Pbkdf2(key, aes.IV, 169000, HashAlgorithmName.SHA256, aes.KeySize / 8);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Compute HMAC-SHA512
        var hmacKey = SHA512.Create().ComputeHash(aes.Key);
        var hmac = new HMACSHA512(hmacKey);
        hmac.TransformBlock(aes.IV, 0, aes.IV.Length, null, 0);
        var buffer = new byte[4096];
        // Transform the rest of the stream with the HMAC
        while (stream.Read(buffer, 0, buffer.Length) > 0)
            hmac.TransformBlock(buffer, 0, buffer.Length, null, 0);
        hmac.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        if (hmac.Hash is not { Length: 64 })
            throw new CryptographicException("HMAC calculation failed");
        stream.Seek(0, SeekOrigin.Begin);

        // Encrypt file
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        stream.Seek(0, SeekOrigin.Begin);
        using var encryptedStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Read);

        using var fileStream = append
            ? File.Open(destination, FileMode.Append, FileAccess.Write)
            : File.Create(destination);

        // Mark file as encrypted with versioning number to allow for retro-compatibility
        fileStream.Write(VersioningNumber, 0, 1);
        fileStream.Write(aes.IV, 0, aes.IV.Length);
        fileStream.Write(hmac.Hash, 0, hmac.Hash.Length);
        encryptedStream.CopyTo(fileStream);

        return string.IsNullOrEmpty(password) ? Convert.ToBase64String(key) : password;
    }

    /// <summary>
    /// Decrypt a file using AES-256-CBC. Supports any size. Includes a HMAC-SHA512 verification to ensure integrity.<br/>
    /// This overload <b>does NOT support</b> legacy decryption. For legacy decryption, use <see cref="DecryptFile(string, string, string, bool)"/>.
    /// </summary>
    /// <param name="stream"><see cref="FileStream"/> to decrypt</param>
    /// <param name="destination">Destination file. Warning, it will overwrite any file with the same name</param>
    /// <param name="password">Plain text password</param>
    /// <param name="overwrite">If true, will force the overwrite of the destination file</param>
    /// <exception cref="FileNotFoundException">File to decrypt does not exist</exception>
    /// <exception cref="CryptographicException">The HMAC verification/calculation has failed</exception>
    public static void DecryptFile(FileStream stream, string destination, string password, bool overwrite = true)
    {
        var tryBase64 = Array.Empty<byte>();
        try
        {
            tryBase64 = Convert.FromBase64String(password);
        }
        catch
        {
            // Ignored ; I use a try/catch here because TryFromBase64String is janky and doesn't work
        }

        var key = tryBase64.Length == 32 ? tryBase64.ToArray() : Encoding.UTF8.GetBytes(password);

        var versionBytes = new byte[1];
        _ = stream.Read(versionBytes, 0, 1);

        // Get the IV from the start of the stream
        var iv = new byte[16];
        _ = stream.Read(iv, 0, 16);

        // Get the HMAC-SHA512 from the start of the stream
        var hmacBytes = new byte[64];
        _ = stream.Read(hmacBytes, 0, 64);

        using var aes = Aes.Create();
        aes.Key = Rfc2898DeriveBytes.Pbkdf2(key, iv, 169000, HashAlgorithmName.SHA256, aes.KeySize / 8);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateDecryptor(aes.Key, iv);

        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        using var fileStream = File.Create(tempFile);

        try
        {
            using var encryptedStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Read);
            encryptedStream.CopyTo(fileStream);
        }
        catch
        {
            File.Delete(tempFile);
            throw;
        }

        // Compute HMAC-SHA512
        var hmacKey = SHA512.Create().ComputeHash(aes.Key);
        var hmac = new HMACSHA512(hmacKey);
        hmac.TransformBlock(iv, 0, iv.Length, null, 0);
        var buffer = new byte[4096];
        fileStream.Seek(0, SeekOrigin.Begin);
        // Transform the rest of the stream with the HMAC
        while (fileStream.Read(buffer, 0, buffer.Length) > 0)
            hmac.TransformBlock(buffer, 0, buffer.Length, null, 0);
        hmac.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        if (hmac.Hash is not { Length: 64 })
        {
            File.Delete(tempFile);
            throw new CryptographicException("HMAC calculation failed");
        }

        if (!hmac.Hash.SequenceEqual(hmacBytes))
        {
            File.Delete(tempFile);
            throw new CryptographicException("HMAC verification failed, file may have been tampered with");
        }

        fileStream.Close();
        File.Move(tempFile, destination, overwrite);
    }

    /// <summary>
    /// Decrypt a file using AES-256-CBC. Supports any size. Includes a HMAC-SHA512 verification to ensure integrity.<br/>
    /// <b>Supports legacy decryption.</b>
    /// </summary>
    /// <param name="filePath">File to decrypt</param>
    /// <param name="destination">Destination file. Warning, it will overwrite any file with the same name</param>
    /// <param name="password">Plain text password</param>
    /// <param name="overwrite">If true, will force the overwrite of the destination file</param>
    /// <exception cref="FileNotFoundException">File to decrypt does not exist</exception>
    /// <exception cref="CryptographicException">The HMAC verification/calculation has failed</exception>
    public static void DecryptFile(string filePath, string destination, string password, bool overwrite = true)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File to decrypt does not exist", filePath);

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        // Get the version from the start of the stream
        var versionBytes = new byte[1];
        _ = stream.Read(versionBytes, 0, 1);

        // If it's an old version, use legacy decryption
        if (versionBytes[0] != VersioningNumber[0])
        {
            stream.Dispose();
            LegacyEncryptionDecryption.DecryptFile(filePath, destination, password, versionBytes[0]);
            return;
        }
        stream.Seek(0, SeekOrigin.Begin);
        DecryptFile(stream, destination, password, overwrite);
    }
}
