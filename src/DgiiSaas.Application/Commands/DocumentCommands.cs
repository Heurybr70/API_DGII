using DgiiSaas.Domain.Enums;
using MediatR;

namespace DgiiSaas.Application.Commands;

// ============================================================
// CreateDocument
// ============================================================
public record CreateDocumentCommand : IRequest<CreateDocumentResult>
{
    public string? IdempotencyKey { get; init; }
    public string? ExternalDocumentId { get; init; }
    public int DocumentType { get; init; }
    public string ENCF { get; init; } = string.Empty;
    public DateTime SequenceExpirationDate { get; init; }
    public int IncomeType { get; init; }
    public int PaymentType { get; init; }

    // Issuer
    public string IssuerRnc { get; init; } = string.Empty;
    public string IssuerBusinessName { get; init; } = string.Empty;
    public string? IssuerTradeName { get; init; }
    public string IssuerAddress { get; init; } = string.Empty;
    public string? IssuerEmail { get; init; }
    public string? IssuerPhone { get; init; }
    public DateTime IssueDate { get; init; }

    // Buyer
    public string? BuyerRnc { get; init; }
    public string? BuyerBusinessName { get; init; }
    public string? BuyerEmail { get; init; }
    public string? BuyerAddress { get; init; }
    public string? ForeignIdentifier { get; init; }

    // Totals
    public decimal TotalAmount { get; init; }
    public decimal? TotalITBIS { get; init; }
    public decimal? TotalTaxableAmount { get; init; }
    public decimal? ExemptAmount { get; init; }

    // Lines
    public List<CreateDocumentLineCommand> Lines { get; init; } = new();
    public List<CreatePaymentDetailCommand>? PaymentDetails { get; init; }

    // Reference
    public string? ModifiedNCF { get; init; }
    public DateTime? ModifiedNCFDate { get; init; }
    public int? ModificationCode { get; init; }

    // Currency
    public string? AlternateCurrencyCode { get; init; }
    public decimal? ExchangeRate { get; init; }
}

public record CreateDocumentLineCommand
{
    public int LineNumber { get; init; }
    public int InvoicingIndicator { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public int GoodOrService { get; init; }
    public string? ItemDescription { get; init; }
    public decimal Quantity { get; init; }
    public int? UnitOfMeasure { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal? DiscountAmount { get; init; }
    public decimal ItemAmount { get; init; }
    public string? ItemCode { get; init; }
    public string? ItemCodeType { get; init; }
}

public record CreatePaymentDetailCommand
{
    public int PaymentForm { get; init; }
    public decimal Amount { get; init; }
}

public record CreateDocumentResult(
    Guid DocumentId,
    string ENCF,
    string Status,
    string Message);

// ============================================================
// SubmitDocument
// ============================================================
public record SubmitDocumentCommand(Guid DocumentId) : IRequest<SubmitDocumentResult>;

public record SubmitDocumentResult(
    bool IsSuccess,
    string? TrackId,
    string Status,
    string Message);

// ============================================================
// CancelDocument
// ============================================================
public record CancelDocumentCommand(Guid DocumentId, string Reason) : IRequest<CancelDocumentResult>;

public record CancelDocumentResult(bool IsSuccess, string Status, string Message);
