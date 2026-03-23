using System.Xml;
using DgiiSaas.Application.Interfaces;
using DgiiSaas.Domain.Entities;
using DgiiSaas.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DgiiSaas.Infrastructure.Services;

public class XmlGeneratorService : IXmlGeneratorService
{
    private readonly ILogger<XmlGeneratorService> _logger;

    public XmlGeneratorService(ILogger<XmlGeneratorService> logger)
    {
        _logger = logger;
    }

    public Task<string> GenerateXmlAsync(ElectronicDocument document, CancellationToken ct = default)
    {
        _logger.LogInformation("Generando XML para e-CF {ENCF}", document.ENCF);

        // DGII XML formats have different root elements based on the e-CF type.
        // For example, e-CF 31 is <ECF>, 32 is <RFCE> or <ECF> depending on the spec, 
        // 33/34/etc are also <ECF>.
        
        var doc = new XmlDocument();
        // Add XML Declaration
        var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
        doc.AppendChild(xmlDeclaration);

        // Root Element ECF
        var rootElement = doc.CreateElement("ECF");
        doc.AppendChild(rootElement);

        // 1. Encabezado
        var encabezado = doc.CreateElement("Encabezado");
        rootElement.AppendChild(encabezado);

        AppendElement(doc, encabezado, "Version", "1.0");
        
        // 1.1 IdDoc
        var idDoc = doc.CreateElement("IdDoc");
        encabezado.AppendChild(idDoc);
        AppendElement(doc, idDoc, "TipoeCF", ((int)document.DocumentType).ToString());
        AppendElement(doc, idDoc, "eNCF", document.ENCF);
        AppendElement(doc, idDoc, "FechaVencimientoSecuencia", document.SequenceExpirationDate.ToString("dd-MM-yyyy"));
        AppendElement(doc, idDoc, "IndicadorEnvioDiferido", document.IsDeferredSend ? "1" : "0");
        AppendElement(doc, idDoc, "IndicadorMontoGravado", document.TaxedAmountIndicator == TaxedAmountIndicator.ConITBISIncluido ? "1" : "0");
        AppendElement(doc, idDoc, "IndicadorServicioTodoIncluido", document.IsAllInclusive ? "1" : "0");
        AppendElement(doc, idDoc, "TipoIngresos", ((int)document.IncomeType).ToString("D2"));
        AppendElement(doc, idDoc, "TipoPago", ((int)document.PaymentType).ToString());
        
        if (document.PaymentDueDate.HasValue)
            AppendElement(doc, idDoc, "FechaLimitePago", document.PaymentDueDate.Value.ToString("dd-MM-yyyy"));
        
        if (!string.IsNullOrEmpty(document.PaymentTerms))
            AppendElement(doc, idDoc, "TerminoPago", document.PaymentTerms);

        // TablaFormasPago
        if (document.PaymentDetails != null && document.PaymentDetails.Any())
        {
            var tablaFormasPago = doc.CreateElement("TablaFormasPago");
            idDoc.AppendChild(tablaFormasPago);

            foreach (var pd in document.PaymentDetails)
            {
                var formaDePago = doc.CreateElement("FormaDePago");
                tablaFormasPago.AppendChild(formaDePago);
                AppendElement(doc, formaDePago, "FormaPago", ((int)pd.PaymentForm).ToString());
                AppendElement(doc, formaDePago, "MontoPago", pd.Amount.ToString("F2"));
            }
        }

        // 1.2 Emisor
        var emisor = doc.CreateElement("Emisor");
        encabezado.AppendChild(emisor);
        AppendElement(doc, emisor, "RNCEmisor", document.IssuerRnc);
        AppendElement(doc, emisor, "RazonSocialEmisor", document.IssuerBusinessName);
        if (!string.IsNullOrEmpty(document.IssuerTradeName)) AppendElement(doc, emisor, "NombreComercial", document.IssuerTradeName);
        AppendElement(doc, emisor, "DireccionEmisor", document.IssuerAddress);
        AppendElement(doc, emisor, "FechaEmision", document.IssueDate.ToString("dd-MM-yyyy"));

        // 1.3 Comprador
        var comprador = doc.CreateElement("Comprador");
        encabezado.AppendChild(comprador);
        if (!string.IsNullOrEmpty(document.BuyerRnc)) AppendElement(doc, comprador, "RNCComprador", document.BuyerRnc);
        if (!string.IsNullOrEmpty(document.BuyerBusinessName)) AppendElement(doc, comprador, "RazonSocialComprador", document.BuyerBusinessName);

        // 1.6 Totales
        var totales = doc.CreateElement("Totales");
        encabezado.AppendChild(totales);
        if (document.TotalTaxableAmount.HasValue) AppendElement(doc, totales, "MontoGravadoTotal", document.TotalTaxableAmount.Value.ToString("F2"));
        if (document.ExemptAmount.HasValue) AppendElement(doc, totales, "MontoExento", document.ExemptAmount.Value.ToString("F2"));
        if (document.TotalITBIS.HasValue) AppendElement(doc, totales, "TotalITBIS", document.TotalITBIS.Value.ToString("F2"));
        AppendElement(doc, totales, "MontoTotal", document.TotalAmount.ToString("F2"));

        // 2. DetallesItems
        var detallesItems = doc.CreateElement("DetallesItems");
        rootElement.AppendChild(detallesItems);

        foreach (var line in document.Lines.OrderBy(l => l.LineNumber))
        {
            var item = doc.CreateElement("Item");
            detallesItems.AppendChild(item);

            AppendElement(doc, item, "NumeroLinea", line.LineNumber.ToString());
            
            if (!string.IsNullOrEmpty(document.BuyerRnc) && !string.IsNullOrEmpty(line.ItemCode))
            {
                 var tablaCodigosItem = doc.CreateElement("TablaCodigosItem");
                 item.AppendChild(tablaCodigosItem);
                 var codigosItem = doc.CreateElement("CodigosItem");
                 tablaCodigosItem.AppendChild(codigosItem);
                 AppendElement(doc, codigosItem, "TipoCodigo", string.IsNullOrEmpty(line.ItemCodeType) ? "INT" : line.ItemCodeType);
                 AppendElement(doc, codigosItem, "CodigoItem", line.ItemCode);
            }

            AppendElement(doc, item, "IndicadorFacturacion", ((int)line.InvoicingIndicator).ToString());
            AppendElement(doc, item, "NombreItem", line.ItemName);
            AppendElement(doc, item, "IndicadorBienoServicio", ((int)line.GoodOrService).ToString());
            
            if(!string.IsNullOrEmpty(line.ItemDescription))
                AppendElement(doc, item, "DescripcionItem", line.ItemDescription);
                
            AppendElement(doc, item, "CantidadItem", line.Quantity.ToString("F2"));
            
            if (line.UnitOfMeasure.HasValue)
                AppendElement(doc, item, "UnidadMedida", line.UnitOfMeasure.Value.ToString());
                
            AppendElement(doc, item, "PrecioUnitarioItem", line.UnitPrice.ToString("F4"));
            
            if (line.DiscountAmount.HasValue && line.DiscountAmount.Value > 0)
                AppendElement(doc, item, "DescuentoMonto", line.DiscountAmount.Value.ToString("F2"));
                
            AppendElement(doc, item, "MontoItem", line.ItemAmount.ToString("F2"));
        }

        // 3. Subtotales, DescuentosORecargos, InformacionReferencia...
        if (document.ModificationCode.HasValue || !string.IsNullOrEmpty(document.ModifiedNCF))
        {
             var informacionReferencia = doc.CreateElement("InformacionReferencia");
             rootElement.AppendChild(informacionReferencia);
             
             if (!string.IsNullOrEmpty(document.ModifiedNCF))
                 AppendElement(doc, informacionReferencia, "NCFModificado", document.ModifiedNCF);
                 
             if (document.ModifiedNCFDate.HasValue)
                 AppendElement(doc, informacionReferencia, "FechaNCFModificado", document.ModifiedNCFDate.Value.ToString("dd-MM-yyyy"));
                 
             if (document.ModificationCode.HasValue)
                 AppendElement(doc, informacionReferencia, "CodigoModificacion", ((int)document.ModificationCode.Value).ToString());
        }

        // 4. FechaHoraFirma
        AppendElement(doc, rootElement, "FechaHoraFirma", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

        using var stringWriter = new System.IO.StringWriter();
        var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = false, OmitXmlDeclaration = false });
        doc.WriteTo(xmlTextWriter);
        xmlTextWriter.Flush();

        return Task.FromResult(stringWriter.GetStringBuilder().ToString());
    }

    private void AppendElement(XmlDocument doc, XmlElement parent, string name, string value)
    {
        var element = doc.CreateElement(name);
        element.InnerText = value;
        parent.AppendChild(element);
    }
}
