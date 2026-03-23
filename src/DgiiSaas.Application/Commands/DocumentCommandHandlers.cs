using DgiiSaas.Application.Interfaces;
using DgiiSaas.Domain.Entities;
using DgiiSaas.Domain.Enums;
using MediatR;

namespace DgiiSaas.Application.Commands;

public class CreateDocumentHandler : IRequestHandler<CreateDocumentCommand, CreateDocumentResult>
{
    private readonly IDocumentRepository _documentRepo;
    private readonly ITenantContext _tenantContext;
    private readonly IDistributedLockService _lockService;

    public CreateDocumentHandler(
        IDocumentRepository documentRepo,
        ITenantContext tenantContext,
        IDistributedLockService lockService)
    {
        _documentRepo = documentRepo;
        _tenantContext = tenantContext;
        _lockService = lockService;
    }

    public async Task<CreateDocumentResult> Handle(CreateDocumentCommand request, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId;

        // Idempotency check
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = await _documentRepo.GetByIdempotencyKeyAsync(tenantId, request.IdempotencyKey, ct);
            if (existing != null)
                return new CreateDocumentResult(existing.Id, existing.ENCF, existing.Status.ToString(), "Documento ya existe (idempotency).");
        }

        // Distributed lock by tenant + eNCF
        var lockKey = $"doc:{tenantId}:{request.ENCF}";
        await using var lck = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(30), ct);
        if (lck == null)
            return new CreateDocumentResult(Guid.Empty, request.ENCF, "Error", "No se pudo adquirir el lock. Intente de nuevo.");

        // Duplicate eNCF check
        var duplicate = await _documentRepo.GetByEncfAsync(tenantId, request.ENCF, ct);
        if (duplicate != null)
            return new CreateDocumentResult(duplicate.Id, duplicate.ENCF, duplicate.Status.ToString(), "eNCF duplicado.");

        // Map to entity
        var document = new ElectronicDocument
        {
            TenantId = tenantId,
            DocumentType = (DocumentType)request.DocumentType,
            ENCF = request.ENCF,
            SequenceExpirationDate = request.SequenceExpirationDate,
            ExternalDocumentId = request.ExternalDocumentId,
            IdempotencyKey = request.IdempotencyKey,
            IncomeType = (IncomeType)request.IncomeType,
            PaymentType = (PaymentType)request.PaymentType,
            IssuerRnc = request.IssuerRnc,
            IssuerBusinessName = request.IssuerBusinessName,
            IssuerTradeName = request.IssuerTradeName,
            IssuerAddress = request.IssuerAddress,
            IssuerEmail = request.IssuerEmail,
            IssuerPhone = request.IssuerPhone,
            IssueDate = request.IssueDate,
            BuyerRnc = request.BuyerRnc,
            BuyerBusinessName = request.BuyerBusinessName,
            BuyerEmail = request.BuyerEmail,
            BuyerAddress = request.BuyerAddress,
            ForeignIdentifier = request.ForeignIdentifier,
            TotalAmount = request.TotalAmount,
            TotalITBIS = request.TotalITBIS,
            TotalTaxableAmount = request.TotalTaxableAmount,
            ExemptAmount = request.ExemptAmount,
            ModifiedNCF = request.ModifiedNCF,
            ModifiedNCFDate = request.ModifiedNCFDate,
            ModificationCode = request.ModificationCode.HasValue ? (ModificationCode)request.ModificationCode.Value : null,
            AlternateCurrencyCode = request.AlternateCurrencyCode,
            ExchangeRate = request.ExchangeRate,
            Status = DocumentStatus.ReceivedFromClient
        };

        // Map lines
        foreach (var line in request.Lines)
        {
            document.Lines.Add(new DocumentLine
            {
                DocumentId = document.Id,
                LineNumber = line.LineNumber,
                InvoicingIndicator = (InvoicingIndicator)line.InvoicingIndicator,
                ItemName = line.ItemName,
                GoodOrService = (GoodOrServiceIndicator)line.GoodOrService,
                ItemDescription = line.ItemDescription,
                Quantity = line.Quantity,
                UnitOfMeasure = line.UnitOfMeasure,
                UnitPrice = line.UnitPrice,
                DiscountAmount = line.DiscountAmount,
                ItemAmount = line.ItemAmount,
                ItemCode = line.ItemCode,
                ItemCodeType = line.ItemCodeType
            });
        }

        // Map payment details
        if (request.PaymentDetails != null)
        {
            foreach (var pd in request.PaymentDetails)
            {
                document.PaymentDetails.Add(new PaymentDetail
                {
                    DocumentId = document.Id,
                    PaymentForm = (PaymentForm)pd.PaymentForm,
                    Amount = pd.Amount
                });
            }
        }

        // Status history
        document.StatusHistory.Add(new DocumentStatusHistory
        {
            DocumentId = document.Id,
            FromStatus = DocumentStatus.Draft,
            ToStatus = DocumentStatus.ReceivedFromClient,
            Reason = "Documento recibido del cliente"
        });

        await _documentRepo.AddAsync(document, ct);
        await _documentRepo.SaveChangesAsync(ct);

        return new CreateDocumentResult(document.Id, document.ENCF, document.Status.ToString(), "Documento creado exitosamente.");
    }
}

public class SubmitDocumentHandler : IRequestHandler<SubmitDocumentCommand, SubmitDocumentResult>
{
    private readonly IDocumentRepository _documentRepo;
    private readonly IXmlGeneratorService _xmlGenerator;
    private readonly IDigitalSignatureService _signatureService;
    private readonly IDgiiAuthService _dgiiAuth;
    private readonly IDgiiSubmissionService _dgiiSubmission;
    private readonly IFileStorageService _fileStorage;
    private readonly IWebhookService _webhookService;
    private readonly ITenantContext _tenantContext;

    public SubmitDocumentHandler(
        IDocumentRepository documentRepo,
        IXmlGeneratorService xmlGenerator,
        IDigitalSignatureService signatureService,
        IDgiiAuthService dgiiAuth,
        IDgiiSubmissionService dgiiSubmission,
        IFileStorageService fileStorage,
        IWebhookService webhookService,
        ITenantContext tenantContext)
    {
        _documentRepo = documentRepo;
        _xmlGenerator = xmlGenerator;
        _signatureService = signatureService;
        _dgiiAuth = dgiiAuth;
        _dgiiSubmission = dgiiSubmission;
        _fileStorage = fileStorage;
        _webhookService = webhookService;
        _tenantContext = tenantContext;
    }

    public async Task<SubmitDocumentResult> Handle(SubmitDocumentCommand request, CancellationToken ct)
    {
        var document = await _documentRepo.GetByIdAsync(request.DocumentId, ct);
        if (document == null)
            return new SubmitDocumentResult(false, null, "Error", "Documento no encontrado.");

        if (document.TenantId != _tenantContext.TenantId)
            return new SubmitDocumentResult(false, null, "Error", "No autorizado.");

        try
        {
            // 1. Generate XML
            var xml = await _xmlGenerator.GenerateXmlAsync(document, ct);
            var xmlPath = await _fileStorage.SaveFileAsync(document.TenantId,
                $"{document.ENCF}.xml", System.Text.Encoding.UTF8.GetBytes(xml), ct);
            document.XmlFilePath = xmlPath;

            // 2. Sign XML
            var signedXml = await _signatureService.SignXmlAsync(xml, document.TenantId, ct);
            var signedXmlPath = await _fileStorage.SaveFileAsync(document.TenantId,
                $"{document.ENCF}_signed.xml", System.Text.Encoding.UTF8.GetBytes(signedXml), ct);
            document.SignedXmlFilePath = signedXmlPath;
            document.SignedAt = DateTime.UtcNow;
            UpdateStatus(document, DocumentStatus.Signed, "Documento firmado.");

            // 3. Authenticate with DGII
            await _dgiiAuth.GetTokenAsync(document.TenantId, ct);
            UpdateStatus(document, DocumentStatus.Authenticated, "Autenticado con DGII.");

            // 4. Submit to DGII
            var result = await _dgiiSubmission.SubmitDocumentAsync(document.TenantId, signedXml, ct);

            document.DgiiSubmissions.Add(new DgiiSubmission
            {
                DocumentId = document.Id,
                TrackId = result.TrackId,
                HttpStatusCode = result.HttpStatusCode,
                ResponseCode = result.ResponseCode,
                ResponseMessage = result.ResponseMessage,
                IsSuccess = result.IsSuccess
            });

            if (result.IsSuccess)
            {
                document.DgiiTrackId = result.TrackId;
                document.SubmittedAt = DateTime.UtcNow;
                UpdateStatus(document, DocumentStatus.TrackIdReceived, $"TrackId: {result.TrackId}");
            }
            else
            {
                UpdateStatus(document, DocumentStatus.SubmissionFailed, result.ResponseMessage ?? "Error en envío.");
            }

            await _documentRepo.UpdateAsync(document, ct);
            await _documentRepo.SaveChangesAsync(ct);

            return new SubmitDocumentResult(result.IsSuccess, result.TrackId,
                document.Status.ToString(), result.ResponseMessage ?? "Enviado.");
        }
        catch (Exception ex)
        {
            UpdateStatus(document, DocumentStatus.SubmissionFailed, ex.Message);
            await _documentRepo.UpdateAsync(document, ct);
            await _documentRepo.SaveChangesAsync(ct);
            return new SubmitDocumentResult(false, null, DocumentStatus.SubmissionFailed.ToString(), ex.Message);
        }
    }

    private static void UpdateStatus(ElectronicDocument doc, DocumentStatus newStatus, string reason)
    {
        var oldStatus = doc.Status;
        doc.Status = newStatus;
        doc.StatusHistory.Add(new DocumentStatusHistory
        {
            DocumentId = doc.Id,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            Reason = reason
        });
    }
}

public class CancelDocumentHandler : IRequestHandler<CancelDocumentCommand, CancelDocumentResult>
{
    private readonly IDocumentRepository _documentRepo;
    private readonly ITenantContext _tenantContext;

    public CancelDocumentHandler(IDocumentRepository documentRepo, ITenantContext tenantContext)
    {
        _documentRepo = documentRepo;
        _tenantContext = tenantContext;
    }

    public async Task<CancelDocumentResult> Handle(CancelDocumentCommand request, CancellationToken ct)
    {
        var document = await _documentRepo.GetByIdAsync(request.DocumentId, ct);
        if (document == null)
            return new CancelDocumentResult(false, "Error", "Documento no encontrado.");

        if (document.TenantId != _tenantContext.TenantId)
            return new CancelDocumentResult(false, "Error", "No autorizado.");

        if (document.Status == DocumentStatus.Cancelled || document.Status == DocumentStatus.CancelRequested)
            return new CancelDocumentResult(false, document.Status.ToString(), "Documento ya cancelado o en proceso de cancelación.");

        var oldStatus = document.Status;
        document.Status = DocumentStatus.CancelRequested;
        document.StatusHistory.Add(new DocumentStatusHistory
        {
            DocumentId = document.Id,
            FromStatus = oldStatus,
            ToStatus = DocumentStatus.CancelRequested,
            Reason = request.Reason
        });

        await _documentRepo.UpdateAsync(document, ct);
        await _documentRepo.SaveChangesAsync(ct);

        return new CancelDocumentResult(true, DocumentStatus.CancelRequested.ToString(), "Cancelación solicitada.");
    }
}
