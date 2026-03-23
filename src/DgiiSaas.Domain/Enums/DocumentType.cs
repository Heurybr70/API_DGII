namespace DgiiSaas.Domain.Enums;

/// <summary>
/// Tipos de e-CF según DGII.
/// </summary>
public enum DocumentType
{
    /// <summary>Factura de Crédito Fiscal Electrónica</summary>
    eCF31 = 31,
    /// <summary>Factura de Consumo Electrónica</summary>
    eCF32 = 32,
    /// <summary>Nota de Débito Electrónica</summary>
    eCF33 = 33,
    /// <summary>Nota de Crédito Electrónica</summary>
    eCF34 = 34,
    /// <summary>Compras Electrónico</summary>
    eCF41 = 41,
    /// <summary>Gastos Menores Electrónico</summary>
    eCF43 = 43,
    /// <summary>Regímenes Especiales Electrónico</summary>
    eCF44 = 44,
    /// <summary>Gubernamental Electrónico</summary>
    eCF45 = 45,
    /// <summary>Comprobante de Exportaciones Electrónico</summary>
    eCF46 = 46,
    /// <summary>Comprobante para Pagos al Exterior Electrónico</summary>
    eCF47 = 47
}
