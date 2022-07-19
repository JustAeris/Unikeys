using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Unikeys.Core.FileSigning;

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
