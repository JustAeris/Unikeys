using System.Security.Cryptography;
using System.Text;

namespace Unikeys.Core.FileEncryption;

public static class Encryption
{
    /// <summary>
    /// Encrypts a file using the AES-256 algorithm.
    /// </summary>
    /// <param name="filePath">File to encrypt</param>
    /// <param name="destination">Decrypted file path</param>
    /// <param name="password">Password to use, if left empty, will generate a strong unique key</param>
    /// <exception cref="FileNotFoundException">Specified file does not exist</exception>
    /// <returns>Used password, if left empty, will be the generated key</returns>
    public static string EncryptFile(string filePath, string destination, string password = "")
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
}
