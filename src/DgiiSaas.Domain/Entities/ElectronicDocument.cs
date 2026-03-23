using DgiiSaas.Domain.Enums;

namespace DgiiSaas.Domain.Entities;

/// <summary>
/// Documento fiscal electrónico canónico principal (e-CF).
/// Modelo desacoplado del XML de DGII.
/// </summary>
public class ElectronicDocument : TenantScopedEntity
{
    // --- Identificación ---
    public DocumentType DocumentType { get; set; }
    public string ENCF { get; set; } = string.Empty;
    public DateTime SequenceExpirationDate { get; set; }
    public string? ExternalDocumentId { get; set; }
    public string? IdempotencyKey { get; set; }

    // --- Estado ---
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    // --- Indicadores ---
    public bool IsDeferredSend { get; set; }
    public TaxedAmountIndicator? TaxedAmountIndicator { get; set; }
    public bool IsAllInclusive { get; set; }
    public IncomeType IncomeType { get; set; }

    // --- Pago ---
    public PaymentType PaymentType { get; set; }
    public DateTime? PaymentDueDate { get; set; }
    public string? PaymentTerms { get; set; }
    public string? PaymentAccountType { get; set; }
    public string? PaymentAccountNumber { get; set; }
    public string? PaymentBank { get; set; }

    // --- Periodo ---
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
    public int? TotalPages { get; set; }

    // --- Emisor (Issuer) ---
    public string IssuerRnc { get; set; } = string.Empty;
    public string IssuerBusinessName { get; set; } = string.Empty;
    public string? IssuerTradeName { get; set; }
    public string? IssuerBranch { get; set; }
    public string IssuerAddress { get; set; } = string.Empty;
    public string? IssuerMunicipality { get; set; }
    public string? IssuerProvince { get; set; }
    public string? IssuerPhone { get; set; }
    public string? IssuerEmail { get; set; }
    public string? IssuerWebSite { get; set; }
    public string? IssuerEconomicActivity { get; set; }
    public string? SellerCode { get; set; }
    public string? InternalInvoiceNumber { get; set; }
    public string? InternalOrderNumber { get; set; }
    public string? SalesZone { get; set; }
    public string? SalesRoute { get; set; }
    public string? IssuerAdditionalInfo { get; set; }
    public DateTime IssueDate { get; set; }

    // --- Comprador (Buyer) ---
    public string? BuyerRnc { get; set; }
    public string? BuyerBusinessName { get; set; }
    public string? BuyerContactName { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerAddress { get; set; }
    public string? BuyerMunicipality { get; set; }
    public string? BuyerProvince { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? DeliveryContact { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BuyerAdditionalPhone { get; set; }
    public DateTime? PurchaseOrderDate { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? BuyerInternalCode { get; set; }
    public string? PaymentResponsible { get; set; }
    public string? BuyerAdditionalInfo { get; set; }
    public string? ForeignIdentifier { get; set; }

    // --- Totales ---
    public decimal? TotalTaxableAmount { get; set; }
    public decimal? TaxableAmountI1 { get; set; }
    public decimal? TaxableAmountI2 { get; set; }
    public decimal? TaxableAmountI3 { get; set; }
    public decimal? ExemptAmount { get; set; }
    public int? ITBIS1Rate { get; set; }
    public int? ITBIS2Rate { get; set; }
    public int? ITBIS3Rate { get; set; }
    public decimal? TotalITBIS { get; set; }
    public decimal? TotalITBIS1 { get; set; }
    public decimal? TotalITBIS2 { get; set; }
    public decimal? TotalITBIS3 { get; set; }
    public decimal? AdditionalTaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? NonBillableAmount { get; set; }
    public decimal? PeriodAmount { get; set; }
    public decimal? PreviousBalance { get; set; }
    public decimal? AdvancePaymentAmount { get; set; }
    public decimal? AmountToPay { get; set; }
    public decimal? TotalITBISRetained { get; set; }
    public decimal? TotalISRRetention { get; set; }
    public decimal? TotalITBISPerception { get; set; }
    public decimal? TotalISRPerception { get; set; }

    // --- Otra Moneda ---
    public string? AlternateCurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? TotalAmountAlternateCurrency { get; set; }

    // --- Referencia ---
    public string? ModifiedNCF { get; set; }
    public string? OtherContributorRnc { get; set; }
    public DateTime? ModifiedNCFDate { get; set; }
    public ModificationCode? ModificationCode { get; set; }

    // --- Firma y DGII ---
    public DateTime? SignedAt { get; set; }
    public string? DgiiTrackId { get; set; }
    public string? DgiiResponseCode { get; set; }
    public string? DgiiResponseMessage { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? SecurityCode { get; set; }

    // --- Transporte ---
    public string? DriverName { get; set; }
    public string? TransportDocument { get; set; }
    public string? VehiclePlate { get; set; }

    // --- Almacenamiento ---
    public string? XmlFilePath { get; set; }
    public string? SignedXmlFilePath { get; set; }
    public string? PdfFilePath { get; set; }

    // --- Navigation ---
    public ICollection<DocumentLine> Lines { get; set; } = new List<DocumentLine>();
    public ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();
    public ICollection<DocumentDiscountCharge> DiscountsCharges { get; set; } = new List<DocumentDiscountCharge>();
    public ICollection<DocumentAdditionalTax> AdditionalTaxes { get; set; } = new List<DocumentAdditionalTax>();
    public ICollection<DocumentStatusHistory> StatusHistory { get; set; } = new List<DocumentStatusHistory>();
    public ICollection<DgiiSubmission> DgiiSubmissions { get; set; } = new List<DgiiSubmission>();
}
