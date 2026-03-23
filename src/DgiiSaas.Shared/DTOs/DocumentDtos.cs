

namespace DgiiSaas.Shared.DTOs;

// ============================================================
// Request DTOs
// ============================================================
public class CreateDocumentRequest
{
    public string? IdempotencyKey { get; set; }
    public string? ExternalDocumentId { get; set; }
    public int DocumentType { get; set; }
    public string ENCF { get; set; } = string.Empty;
    public DateTime SequenceExpirationDate { get; set; }
    public int IncomeType { get; set; }
    public int PaymentType { get; set; }

    public IssuerDto Issuer { get; set; } = new();
    public BuyerDto? Buyer { get; set; }
    public TotalsDto Totals { get; set; } = new();
    public List<LineDto> Lines { get; set; } = new();
    public List<PaymentDetailDto>? PaymentDetails { get; set; }
    public ReferenceDto? Reference { get; set; }
    public CurrencyDto? AlternateCurrency { get; set; }
}

public class IssuerDto
{
    public string Rnc { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime IssueDate { get; set; }
}

public class BuyerDto
{
    public string? Rnc { get; set; }
    public string? BusinessName { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? ForeignIdentifier { get; set; }
}

public class TotalsDto
{
    public decimal TotalAmount { get; set; }
    public decimal? TotalITBIS { get; set; }
    public decimal? TotalTaxableAmount { get; set; }
    public decimal? ExemptAmount { get; set; }
    public decimal? TaxableAmountI1 { get; set; }
    public decimal? TaxableAmountI2 { get; set; }
    public decimal? TaxableAmountI3 { get; set; }
    public decimal? TotalITBIS1 { get; set; }
    public decimal? TotalITBIS2 { get; set; }
    public decimal? TotalITBIS3 { get; set; }
}

public class LineDto
{
    public int LineNumber { get; set; }
    public int InvoicingIndicator { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int GoodOrService { get; set; }
    public string? ItemDescription { get; set; }
    public decimal Quantity { get; set; }
    public int? UnitOfMeasure { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal ItemAmount { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemCodeType { get; set; }
}

public class PaymentDetailDto
{
    public int PaymentForm { get; set; }
    public decimal Amount { get; set; }
}

public class ReferenceDto
{
    public string? ModifiedNCF { get; set; }
    public DateTime? ModifiedNCFDate { get; set; }
    public int? ModificationCode { get; set; }
}

public class CurrencyDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
}

public class CancelDocumentRequest
{
    public string Reason { get; set; } = string.Empty;
}

// ============================================================
// Response DTOs
// ============================================================
public class DocumentResponse
{
    public Guid Id { get; set; }
    public string ENCF { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? BuyerRnc { get; set; }
    public string? BuyerBusinessName { get; set; }
    public string? TrackId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DocumentStatusResponse
{
    public Guid DocumentId { get; set; }
    public string ENCF { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TrackId { get; set; }
    public string? DgiiResponseCode { get; set; }
    public string? DgiiResponseMessage { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<StatusHistoryDto> StatusHistory { get; set; } = new();
}

public class StatusHistoryDto
{
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime TransitionDate { get; set; }
}

// ============================================================
// API Error Response
// ============================================================
public class ApiErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<ApiErrorDetail>? Details { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ApiErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
}

// ============================================================
// Success Response Wrapper
// ============================================================
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
