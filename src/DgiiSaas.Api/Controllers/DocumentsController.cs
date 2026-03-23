using DgiiSaas.Application.Commands;
using DgiiSaas.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DgiiSaas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crea un nuevo documento electrónico (e-CF)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateDocumentResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest dto)
    {
        // En una implementación real se usaría AutoMapper o Mapster.
        // Aquí hacemos un mapeo manual ilustrativo para el prototipo.
        var command = new CreateDocumentCommand
        {
            IdempotencyKey = dto.IdempotencyKey,
            ExternalDocumentId = dto.ExternalDocumentId,
            DocumentType = dto.DocumentType,
            ENCF = dto.ENCF,
            SequenceExpirationDate = dto.SequenceExpirationDate,
            IncomeType = dto.IncomeType,
            PaymentType = dto.PaymentType,
            IssuerRnc = dto.Issuer.Rnc,
            IssuerBusinessName = dto.Issuer.BusinessName,
            IssuerTradeName = dto.Issuer.TradeName,
            IssuerAddress = dto.Issuer.Address,
            IssuerEmail = dto.Issuer.Email,
            IssuerPhone = dto.Issuer.Phone,
            IssueDate = dto.Issuer.IssueDate,
            BuyerRnc = dto.Buyer?.Rnc,
            BuyerBusinessName = dto.Buyer?.BusinessName,
            BuyerEmail = dto.Buyer?.Email,
            BuyerAddress = dto.Buyer?.Address,
            ForeignIdentifier = dto.Buyer?.ForeignIdentifier,
            TotalAmount = dto.Totals.TotalAmount,
            TotalITBIS = dto.Totals.TotalITBIS,
            TotalTaxableAmount = dto.Totals.TotalTaxableAmount,
            ExemptAmount = dto.Totals.ExemptAmount,
            Lines = dto.Lines.Select(l => new CreateDocumentLineCommand
            {
                LineNumber = l.LineNumber,
                InvoicingIndicator = l.InvoicingIndicator,
                ItemName = l.ItemName,
                GoodOrService = l.GoodOrService,
                ItemDescription = l.ItemDescription,
                Quantity = l.Quantity,
                UnitOfMeasure = l.UnitOfMeasure,
                UnitPrice = l.UnitPrice,
                DiscountAmount = l.DiscountAmount,
                ItemAmount = l.ItemAmount,
                ItemCode = l.ItemCode,
                ItemCodeType = l.ItemCodeType
            }).ToList(),
            PaymentDetails = dto.PaymentDetails?.Select(p => new CreatePaymentDetailCommand
            {
                PaymentForm = p.PaymentForm,
                Amount = p.Amount
            }).ToList(),
            ModifiedNCF = dto.Reference?.ModifiedNCF,
            ModifiedNCFDate = dto.Reference?.ModifiedNCFDate,
            ModificationCode = dto.Reference?.ModificationCode,
            AlternateCurrencyCode = dto.AlternateCurrency?.CurrencyCode,
            ExchangeRate = dto.AlternateCurrency?.ExchangeRate
        };

        var result = await _mediator.Send(command);

        if (result.DocumentId == Guid.Empty)
        {
            return BadRequest(new ApiErrorResponse { Code = "DOC_CREATE_ERR", Message = result.Message });
        }

        return CreatedAtAction(nameof(GetDocumentStatus), new { id = result.DocumentId }, new ApiResponse<CreateDocumentResult>
        {
            Success = true,
            Data = result,
            Message = "Documento creado y pendiente de envío."
        });
    }

    /// <summary>
    /// Consulta el estado de un documento electrónico
    /// </summary>
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetDocumentStatus(Guid id)
    {
        // En la implementación real esto sería una Query (GetDocumentStatusQuery)
        // Por motivos ilustrativos del prototipo devolvemos un 200 genérico simulado si MediatR 
        // no tiene un Handler registrado aún en el plan actual de creación.
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = new { DocumentId = id, Status = "Draft o ya enviado", TrackId = (string?)null }
        });
    }

    /// <summary>
    /// Envia el documento asíncronamente (Endpoint para pruebas directas sin Hangfire)
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitDocument(Guid id)
    {
        var result = await _mediator.Send(new SubmitDocumentCommand(id));
        if (!result.IsSuccess)
        {
             return BadRequest(new ApiErrorResponse { Code = "SUBMIT_ERR", Message = result.Message });
        }
        
        return Ok(new ApiResponse<SubmitDocumentResult> { Success = true, Data = result, Message = result.Message });
    }

    /// <summary>
    /// Solicita la anulación de un documento
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelDocument(Guid id, [FromBody] CancelDocumentRequest req)
    {
        var result = await _mediator.Send(new CancelDocumentCommand(id, req.Reason));
        if (!result.IsSuccess)
            return BadRequest(new ApiErrorResponse { Code = "CANCEL_ERR", Message = result.Message });

        return Ok(new ApiResponse<CancelDocumentResult> { Success = true, Data = result, Message = result.Message });
    }
}
