using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MiniProjectTwo.Shared;

public class CryptoUtils
{
    public static X509Certificate CreateOrOpenCertificate(string subjectName)
    {
        var certificateFileName = $"{subjectName}.pfx";

        if (File.Exists(certificateFileName))
        {
            return new X509Certificate(certificateFileName);
        }

        var ecdsa = ECDsa.Create();
        var request = new CertificateRequest($"CN={subjectName}", ecdsa, HashAlgorithmName.SHA256);
        var certificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));

        var certificateBytes = certificate.Export(X509ContentType.Pfx);
        File.WriteAllBytes(certificateFileName, certificateBytes);

        return certificate;
    }
}
