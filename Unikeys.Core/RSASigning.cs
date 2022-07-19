using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace Unikeys.Core;

// TODO: Split the classes in different files.

/// <summary>
/// Methods for signing and verifying files.
/// </summary>
public static class RSASigning
{
    public static RSASignature SignData(byte[] data, X509Certificate2 certificate)
    {
        var rsa = certificate.GetRSAPrivateKey();
        if (rsa == null)
            throw new CryptographicException("Certificate does not contain a private key");

        return new RSASignature(rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1),
            certificate.GetRSAPublicKey() ?? throw new CryptographicException("Certificate does not contain a public key")); ;
    }

    public static bool VerifySignature(byte[] data, RSASignature signature) =>
        signature.PublicKey.VerifyData(data, signature.Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
}

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

/// <summary>
/// Custom class to contain the data required to verify a file
/// </summary>
public class RSASignature
{
    private RSASignature()
    {
        Signature = Array.Empty<byte>();
        PublicKey = RSA.Create();
    }

    public RSASignature(byte[] signature, RSA publicKey)
    {
        Signature = signature;
        PublicKey = publicKey;
    }

    public byte[] Signature { get; private set; }
    public RSA PublicKey { get; }

    public string ToXmlString()
    {
        var xmlDoc = new XmlDocument();
        var root = xmlDoc.CreateElement("RsaSignature");
        xmlDoc.AppendChild(root);
        var signature = xmlDoc.CreateElement("Signature");
        signature.InnerText = Convert.ToBase64String(Signature);
        root.AppendChild(signature);
        var publicKey = xmlDoc.CreateElement("PublicKey");
        publicKey.InnerText = PublicKey.ToXmlString(false);
        root.AppendChild(publicKey);
        return xmlDoc.OuterXml;
    }

    public static RSASignature FromXmlString(string xml)
    {
        var rsaSignature = new RSASignature();
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        var signatureNode = xmlDoc.SelectSingleNode("/RsaSignature/Signature");
        var publicKeyNode = xmlDoc.SelectSingleNode("/RsaSignature/PublicKey");
        rsaSignature.Signature = Convert.FromBase64String(signatureNode?.InnerText ?? string.Empty);
        rsaSignature.PublicKey.FromXmlString(publicKeyNode?.InnerText ?? string.Empty);
        return rsaSignature;
    }
}
