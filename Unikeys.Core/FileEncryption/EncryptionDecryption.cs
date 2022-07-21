using System.Security.Cryptography;
using System.Text;

namespace Unikeys.Core.FileEncryption;

/// <summary>
/// Basic implementation of <see cref="Aes"/> encryption/decryption.
/// </summary>
public static class EncryptionDecryption
{
    /// <summary>
    /// Encrypt a file using AES-256-CBC. Supports any size.
    /// </summary>
    /// <param name="filePath">File to encrypt</param>
    /// <param name="destination">Destination file. Warning, it will overwrite any file with the same name</param>
    /// <param name="password">Optional, program will return a strong key if left empty</param>
    /// <exception cref="FileNotFoundException">Specified file does not exist</exception>
    /// <returns>Used password</returns>
    public static string EncryptFile(string filePath, string destination, string password = "")
    {
        var key = string.IsNullOrEmpty(password) ? RandomNumberGenerator.GetBytes(32) : Encoding.UTF8.GetBytes(password);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

        using var aes = Aes.Create();
        aes.Key = Rfc2898DeriveBytes.Pbkdf2(key, aes.IV, 1069, HashAlgorithmName.SHA256, aes.KeySize / 8);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var encryptedStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Read);
        using var fileStream = File.Create(destination);

        fileStream.Write(aes.IV, 0, aes.IV.Length);
        encryptedStream.CopyTo(fileStream);

        return string.IsNullOrEmpty(password) ? Convert.ToBase64String(key) : password;
    }

    /// <summary>
    /// Decrypt a file using AES-256-CBC. Supports any size.
    /// </summary>
    /// <param name="filePath">File to decrypt</param>
    /// <param name="destination">Destination file. Warning, it will overwrite any file with the same name</param>
    /// <param name="password">Plain text password</param>
    /// <exception cref="FileNotFoundException"></exception>
    public static void DecryptFile(string filePath, string destination, string password)
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

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

        // Get the IV from the start of the stream
        var iv = new byte[16];
        _ = stream.Read(iv, 0, 16);

        using var aes = Aes.Create();
        aes.Key = Rfc2898DeriveBytes.Pbkdf2(key, iv, 1069, HashAlgorithmName.SHA256, aes.KeySize / 8);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var encryptor = aes.CreateDecryptor(aes.Key, iv);

        using var encryptedStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Read);
        using var fileStream = File.Create(destination);

        encryptedStream.CopyTo(fileStream);
    }
}
