using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Unikeys.Core.FileSigning;

/// <summary>
/// Methods to generate and write certificates.
/// </summary>
public static class X509Helper
{
    /// <summary>
    /// Generate a new RSA key pair and return the public and private keys in a X.509 Certificate.
    /// </summary>
    /// <param name="cn">Common Name to use for the certificate</param>
    /// <returns>A certificate containing a public and private key</returns>
    public static X509Certificate2 GenerateX509Certificate(string cn)
    {
        var rsa = RSA.Create(4096);

        var request = new CertificateRequest($"CN={cn}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var publicX509Certificate2 = request.Create(new X500DistinguishedName($"CN={cn}"), X509SignatureGenerator.CreateForRSA(rsa, RSASignaturePadding.Pkcs1),
            DateTimeOffset.UtcNow, DateTimeOffset.MaxValue, Guid.NewGuid().ToByteArray());

        var privateX509Certificate2 = publicX509Certificate2.CopyWithPrivateKey(rsa);
        return privateX509Certificate2;
    }

    /// <summary>
    /// Writes the certificate to a PFX file.
    /// </summary>
    /// <param name="certificate">Certificate to write</param>
    /// <param name="fileName">File path</param>
    public static void WriteX509Certificate(X509Certificate2 certificate, string fileName) =>
        File.WriteAllBytes(fileName, certificate.Export(X509ContentType.Pfx));

    /// <summary>
    /// Reads a certificate from a PFX file.
    /// </summary>
    /// <param name="fileName">File path</param>
    /// <returns>Read certificate</returns>
    public static X509Certificate2 GetCertificateFromPfx(string fileName) => new(fileName);
}
