using System.Security.Cryptography;
using System.Xml;

namespace Unikeys.Core.FileSigning;

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
