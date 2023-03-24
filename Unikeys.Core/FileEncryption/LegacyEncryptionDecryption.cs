using System.Security.Cryptography;
using System.Text;
// ReSharper disable UnusedMember.Local

namespace Unikeys.Core.FileEncryption;

/// <summary>
/// This class will be used for retro-compatibility with old versions of the application.
/// </summary>
internal static class LegacyEncryptionDecryption
{
    /// <summary>
    /// Wrapper for <see cref="EncryptFileV0"/> and <see cref="EncryptFileV1"/>.
    /// </summary>
    /// <param name="filePath">File path to process</param>
    /// <param name="destination">Destination's file path</param>
    /// <param name="password">Password used for encryption</param>
    /// <param name="version">Version number, to allow the use of the correct method</param>
    /// <exception cref="ArgumentOutOfRangeException">Version does not exist</exception>
#pragma warning disable CS0618
    public static void DecryptFile(string filePath, string destination, string password, int version)
    {
        switch (version)
        {
            case 0:
                DecryptFileV0(filePath, destination, password);
                break;
            case 1:
                DecryptFileV1(filePath, destination, password);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(version), version, "Invalid version number");
        }
    }
#pragma warning restore CS0618

    #region Version 1

    /// <summary>
    /// Encrypt a file using AES-256-CBC. Supports any size.
    /// </summary>
    /// <param name="filePath">File to encrypt</param>
    /// <param name="destination">Destination file. Warning, it will overwrite any file with the same name</param>
    /// <param name="password">Optional, program will return a strong key if left empty</param>
    /// <exception cref="FileNotFoundException">Specified file does not exist</exception>
    /// <returns>Used password</returns>
    [Obsolete("This method is deprecated, please use the new version")]
    private static string EncryptFileV1(string filePath, string destination, string password = "")
    {
        var key = string.IsNullOrEmpty(password) ? RandomNumberGenerator.GetBytes(32) : Encoding.UTF8.GetBytes(password);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

        using var aes = Aes.Create();
        aes.Key = Rfc2898DeriveBytes.Pbkdf2(key, aes.IV, 169000, HashAlgorithmName.SHA256, aes.KeySize / 8);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var encryptedStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Read);
        using var fileStream = File.Create(destination);

        // Mark file as encrypted with versioning number to allow for retro-compatibility
        fileStream.Write(new []{Convert.ToByte(1)}, 0, 1);
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
    [Obsolete("This method is deprecated, please use the new version")]
    private static void DecryptFileV1(string filePath, string destination, string password)
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

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        // Get the version from the start of the stream
        var versionBytes = new byte[1];
        _ = stream.Read(versionBytes, 0, 1);

        // Get the IV from the start of the stream
        var iv = new byte[16];
        _ = stream.Read(iv, 0, 16);

        using var aes = Aes.Create();
        aes.Key = Rfc2898DeriveBytes.Pbkdf2(key, iv, 169000, HashAlgorithmName.SHA256, aes.KeySize / 8);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateDecryptor(aes.Key, iv);

        using var encryptedStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Read);
        using var fileStream = File.Create(destination);

        encryptedStream.CopyTo(fileStream);
    }

    #endregion

    #region Version 0

    /// <summary>
    /// Encrypts a file using the AES-256 algorithm.<br/>
    /// Please note it <b>supports only files under 2Gb</b>.
    /// </summary>
    /// <param name="filePath">File to encrypt</param>
    /// <param name="destination">Decrypted file path</param>
    /// <param name="password">Password to use, if left empty, will generate a strong unique key</param>
    /// <exception cref="FileNotFoundException">Specified file does not exist</exception>
    /// <returns>Used password, if left empty, will be the generated key</returns>
    [Obsolete("This method is deprecated, please use the new version")]
    private static string EncryptFileV0(string filePath, string destination, string password = "")
    {
        var key = string.IsNullOrEmpty(password) ? RandomNumberGenerator.GetBytes(32) : Encoding.UTF8.GetBytes(password);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var fileBytes = File.ReadAllBytes(filePath);

        var encryptedBytes = Encrypt(fileBytes, key);

        File.WriteAllBytes(destination, encryptedBytes);

        return string.IsNullOrEmpty(password) ? Convert.ToBase64String(key) : password;
    }

    /// <summary>
    /// Encrypts a set of bytes using AES-256-CBC.
    /// </summary>
    /// <param name="bytes">Data to encrypt</param>
    /// <param name="password">Key to use</param>
    /// <returns>Encrypted data with the IV as the last 16 bytes</returns>
    [Obsolete("This method is deprecated, please use the new version")]
    private static byte[] Encrypt(byte[] bytes, byte[] password)
    {
        if (bytes.Length == 0)
            throw new ArgumentException("Data to encrypt is empty", nameof(bytes));

        using var aes = Aes.Create();

        aes.Key = Rfc2898DeriveBytes.Pbkdf2(password, aes.IV, 1069, HashAlgorithmName.SHA256, aes.KeySize / 8);

        var result = aes.EncryptCbc(bytes, aes.IV);

        // Add the IV to the end of the encrypted data
        var resultWithIv = new byte[result.Length + aes.IV.Length];
        result.CopyTo(resultWithIv, 0);
        aes.IV.CopyTo(resultWithIv, result.Length);

        return resultWithIv;
    }

    /// <summary>
    /// Decrypts a file using the AES-256 algorithm.<br/>
    /// Please note it <b>supports only files under 2Gb</b>.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="destination"></param>
    /// <param name="password"></param>
    /// <exception cref="FileNotFoundException">Specified file does not exist</exception>
    [Obsolete("This method is deprecated, please use the new version")]
    private static void DecryptFileV0(string filePath, string destination, string password)
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

        var fileBytes = File.ReadAllBytes(filePath);

        var decryptedBytes = Decrypt(fileBytes, key);

        File.WriteAllBytes(destination, decryptedBytes);
    }

    /// <summary>
    /// Decrypts a set of bytes using AES-256-CBC.
    /// The last 16 bytes are used as the IV.
    /// </summary>
    /// <param name="bytes">Bytes to decrypt</param>
    /// <param name="password">Key to use</param>
    /// <exception cref="ArgumentException">Bytes array must be at least 16 bytes to be decrypted</exception>
    /// <returns>Returns the raw decrypted data</returns>
    [Obsolete("This method is deprecated, please use the new version")]
    private static byte[] Decrypt(byte[] bytes, byte[] password)
    {
        if (bytes.Length < 16)
            throw new ArgumentException("Data to decrypt is too short", nameof(bytes));

        var iv = new byte[16];
        Array.Copy(bytes, bytes.Length - 16, iv, 0, 16);

        using var aes = Aes.Create();

        aes.Key = Rfc2898DeriveBytes.Pbkdf2(password, iv, 1069, HashAlgorithmName.SHA256, aes.KeySize / 8);

        // Remove the IV from the end of the encrypted data
        var result = new byte[bytes.Length - 16];
        Array.Copy(bytes, 0, result, 0, result.Length);

        return aes.DecryptCbc(result, iv);
    }

    #endregion
}
