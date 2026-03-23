using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using DgiiSaas.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DgiiSaas.Infrastructure.Services;

public class DigitalSignatureService : IDigitalSignatureService
{
    private readonly ICertificateRepository _certificateRepo;
    private readonly ILogger<DigitalSignatureService> _logger;

    public DigitalSignatureService(ICertificateRepository certificateRepo, ILogger<DigitalSignatureService> logger)
    {
        _certificateRepo = certificateRepo;
        _logger = logger;
    }

    public async Task<string> SignXmlAsync(string xml, Guid tenantId, CancellationToken ct = default)
    {
        _logger.LogInformation("Firmando XML para tenant {TenantId}", tenantId);

        var certEntity = await _certificateRepo.GetActiveAsync(tenantId, ct);
        if (certEntity == null)
            throw new Exception("No hay un certificado activo válido para este tenant.");

        // In a real implementation:
        // 1. Decrypt EncryptedCertificateData and EncryptedPassword (e.g., using AWS KMS or Azure KeyVault)
        // 2. Load the X509Certificate2
        
        // Simulating the loading for the prototype:
        byte[] rawData = Convert.FromBase64String(certEntity.EncryptedCertificateData);
        string password = certEntity.EncryptedPassword; // Should be actually decrypted in real scenario
        
        try
        {
            var cert = new X509Certificate2(rawData, password, X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(xml);

            var signedXml = new SignedXml(doc)
            {
                SigningKey = cert.GetRSAPrivateKey()
            };

            var reference = new Reference { Uri = "" }; // Enroll the entire document
            
            // Add EnvelopedSignatureTransform
            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);
            signedXml.AddReference(reference);

            // Add KeyInfo
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;

            // Compute the signature
            signedXml.ComputeSignature();

            // Append the signature to the XML document
            var xmlDigitalSignature = signedXml.GetXml();
            doc.DocumentElement?.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            return doc.OuterXml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error firmando XML. Usando placeholder de prototipo.");
            
            // Reemplazo robusto (case-insensitive) para el tag de cierre del root
            int lastClosingTag = xml.LastIndexOf("</", StringComparison.OrdinalIgnoreCase);
            if (lastClosingTag > 0)
            {
                 return xml.Insert(lastClosingTag, "  <Signature><Dummy>PrototypeSignature_CheckLogs</Dummy></Signature>\n");
            }
            return xml + "<!-- Error signing: " + ex.Message + " -->";
        }
    }
}
