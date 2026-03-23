using DgiiSaas.Domain.Enums;

namespace DgiiSaas.Domain.Entities;

/// <summary>
/// Detalle de forma de pago.
/// </summary>
public class PaymentDetail : BaseEntity
{
    public Guid DocumentId { get; set; }
    public ElectronicDocument Document { get; set; } = null!;

    public PaymentForm PaymentForm { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Descuento o recargo a nivel de documento.
/// </summary>
public class DocumentDiscountCharge : BaseEntity
{
    public Guid DocumentId { get; set; }
    public ElectronicDocument Document { get; set; } = null!;

    public int LineNumber { get; set; }
    public AdjustmentType AdjustmentType { get; set; }
    public bool? IsNorma1007 { get; set; }
    public string? Description { get; set; }
    public DiscountChargeValueType? ValueType { get; set; }
    public decimal? ValuePercentage { get; set; }
    public decimal? Amount { get; set; }
    public decimal? AmountAlternateCurrency { get; set; }
    public InvoicingIndicator? InvoicingIndicator { get; set; }
}

/// <summary>
/// Impuesto adicional del documento.
/// </summary>
public class DocumentAdditionalTax : BaseEntity
{
    public Guid DocumentId { get; set; }
    public ElectronicDocument Document { get; set; } = null!;

    public string TaxType { get; set; } = string.Empty;
    public decimal? TaxRate { get; set; }
    public decimal? SpecificConsumptionTaxAmount { get; set; }
    public decimal? AdValoremConsumptionTaxAmount { get; set; }
    public decimal? OtherAdditionalTaxes { get; set; }
}

/// <summary>
/// Historial de transiciones de estado del documento.
/// </summary>
public class DocumentStatusHistory : BaseEntity
{
    public Guid DocumentId { get; set; }
    public ElectronicDocument Document { get; set; } = null!;

    public DocumentStatus FromStatus { get; set; }
    public DocumentStatus ToStatus { get; set; }
    public string? Reason { get; set; }
    public string? Details { get; set; }
    public DateTime TransitionDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Registro de envío a DGII.
/// </summary>
public class DgiiSubmission : BaseEntity
{
    public Guid DocumentId { get; set; }
    public ElectronicDocument Document { get; set; } = null!;

    public string? TrackId { get; set; }
    public string? RequestXml { get; set; }
    public string? ResponseXml { get; set; }
    public int HttpStatusCode { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public bool IsSuccess { get; set; }
    public int AttemptNumber { get; set; } = 1;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
