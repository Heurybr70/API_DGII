namespace DgiiSaas.Domain.Enums;

/// <summary>Tipo de ingresos.</summary>
public enum IncomeType
{
    OperacionalesNoFinancieros = 1,
    Financieros = 2,
    Extraordinarios = 3,
    Arrendamientos = 4,
    VentaActivoDepreciable = 5,
    OtrosIngresos = 6
}

/// <summary>Indicador de facturación (ITBIS).</summary>
public enum InvoicingIndicator
{
    NoFacturable = 0,
    ITBIS1_18 = 1,
    ITBIS2_16 = 2,
    ITBIS3_0 = 3,
    Exento = 4
}

/// <summary>Indicador de bien o servicio.</summary>
public enum GoodOrServiceIndicator
{
    Bien = 1,
    Servicio = 2
}

/// <summary>Indicador de monto gravado.</summary>
public enum TaxedAmountIndicator
{
    SinITBISIncluido = 0,
    ConITBISIncluido = 1
}

/// <summary>Código de modificación para documentos de referencia.</summary>
public enum ModificationCode
{
    AnulaNCFModificado = 1,
    CorrigeTexto = 2,
    CorrigeMontos = 3,
    ReemplazoContingencia = 4,
    ReferenciaFacturaConsumo = 5
}

/// <summary>Tipo de ajuste (descuento o recargo).</summary>
public enum AdjustmentType
{
    Descuento,
    Recargo
}

/// <summary>Tipo de valor para descuento/recargo.</summary>
public enum DiscountChargeValueType
{
    Monto,
    Porcentaje
}

/// <summary>Indicador de agente de retención o percepción.</summary>
public enum RetentionPerceptionIndicator
{
    Retencion = 1,
    Percepcion = 2
}

/// <summary>Ambiente de operación.</summary>
public enum OperationEnvironment
{
    Sandbox = 0,
    Production = 1
}

/// <summary>Estado de acuse de recibo.</summary>
public enum AcknowledgmentStatus
{
    Recibido = 0,
    NoRecibido = 1
}

/// <summary>Motivo de no recibido.</summary>
public enum NotReceivedReasonCode
{
    ErrorEspecificacion = 1,
    ErrorFirmaDigital = 2,
    EnvioDuplicado = 3,
    RNCCompradorNoCorresponde = 4
}

/// <summary>Estado de aprobación comercial.</summary>
public enum CommercialApprovalStatus
{
    Aceptado = 1,
    Rechazado = 2
}
