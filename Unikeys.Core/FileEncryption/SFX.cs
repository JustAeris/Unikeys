using System.Security.Cryptography;

namespace Unikeys.Core.FileEncryption;

public static class SFX
{
    /// <summary>
    /// 4096 fixed pattern generated with <see cref="RandomNumberGenerator"/> to determine when to start decrypting.<br/>
    /// </summary>
    private const string Marker =
        "euHVouWPWz3PrhiwFsoUZXpdindblfdeliUBu5uwMN7dnb3+jGkF4OwT8UK7Cf5PoM93RKbAvHbBVGhBqbiOLUPyRtJc9rZQvEihLjDy9zKL" +
        #region Marker
        "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" +
        #endregion
        "Cpwn3IP4danGr07iaWAUwoPWLEwu1lSZCicBMWAUZjITAh4RBHn4lig9f98aNw==";

    private const string SFXFileName = "Unikeys.SelfDecrypt.Console.exe";

    private static IEnumerable<byte> GetMarkerBytes() => Convert.FromBase64String(Marker);

    static SFX()
    {
        if (!File.Exists(SFXFileName))
            throw new FileNotFoundException("SFX module not found");
    }

    /// <summary>
    /// Creates a self-decryptable file
    /// </summary>
    /// <param name="sourceFile">Source file</param>
    /// <param name="password">Password for encryption</param>
    /// <returns>Strong key, if password was empty</returns>
    public static string Encrypt(string sourceFile, string password = "")
    {
        var destFile = new FileInfo(sourceFile).Name + ".exe";
        File.Copy(new FileInfo(SFXFileName).FullName, destFile, true);

        // Append marker
        var stream = new FileStream(destFile, FileMode.Append, FileAccess.Write);
        stream.Write(GetMarkerBytes().ToArray(), 0, 4096);
        stream.Close();

        return EncryptionDecryption.EncryptFile(sourceFile, destFile, password, true);
    }

    /// <summary>
    /// Transforms a file into a self-decryptable file
    /// </summary>
    /// <param name="sourceFile">Source file to modify</param>
    /// <param name="destFile">Destination file path</param>
    public static void MakeSFX(string sourceFile, string destFile)
    {
        File.Copy(new FileInfo(SFXFileName).FullName, destFile, true);

        // Append marker
        using var stream = new FileStream(destFile, FileMode.Append, FileAccess.Write);
        stream.Write(GetMarkerBytes().ToArray(), 0, 4096);

        // Append file
        using var fileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
        fileStream.CopyTo(stream);
    }
}
