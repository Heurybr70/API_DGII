using DgiiSaas.Domain.Enums;

namespace DgiiSaas.Domain.Entities;

/// <summary>
/// Línea de detalle del documento (Item).
/// </summary>
public class DocumentLine : BaseEntity
{
    public Guid DocumentId { get; set; }
    public ElectronicDocument Document { get; set; } = null!;

    public int LineNumber { get; set; }
    public InvoicingIndicator InvoicingIndicator { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public GoodOrServiceIndicator GoodOrService { get; set; }
    public string? ItemDescription { get; set; }
    public decimal Quantity { get; set; }
    public int? UnitOfMeasure { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? SurchargeAmount { get; set; }
    public decimal ItemAmount { get; set; }

    // --- Códigos de Item ---
    public string? ItemCodeType { get; set; }
    public string? ItemCode { get; set; }

    // --- Retención ---
    public RetentionPerceptionIndicator? RetentionIndicator { get; set; }
    public decimal? ITBISRetainedAmount { get; set; }
    public decimal? ISRRetainedAmount { get; set; }

    // --- Referencia ---
    public decimal? ReferenceQuantity { get; set; }
    public int? ReferenceUnit { get; set; }
    public decimal? ReferenceUnitPrice { get; set; }

    // --- Fechas Item ---
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // --- Alcohol ---
    public decimal? AlcoholDegrees { get; set; }

    // --- Impuesto Adicional ---
    public string? AdditionalTaxType { get; set; }

    // --- Otra Moneda ---
    public decimal? PriceAlternateCurrency { get; set; }
    public decimal? DiscountAlternateCurrency { get; set; }
    public decimal? SurchargeAlternateCurrency { get; set; }
    public decimal? AmountAlternateCurrency { get; set; }
}
