using System.Security.Cryptography;
using System.Text;

namespace Unikeys.Core.FileEncryption;

public static class Decryption
{
    /// <summary>
    /// Decrypts a file using the AES-256 algorithm.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="destination"></param>
    /// <param name="password"></param>
    /// <exception cref="FileNotFoundException">Specified file does not exist</exception>
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
}
