namespace DgiiSaas.Domain.Enums;

/// <summary>Tipo de pago.</summary>
public enum PaymentType
{
    Contado = 1,
    Credito = 2,
    Gratuito = 3
}

/// <summary>Forma de pago.</summary>
public enum PaymentForm
{
    Efectivo = 1,
    ChequeTransferenciaDeposito = 2,
    TarjetaDebitoCredito = 3,
    VentaCredito = 4,
    BonosCertificadosRegalo = 5,
    Permuta = 6,
    NotaDeCredito = 7,
    OtrasFormasPago = 8
}

/// <summary>Tipo de cuenta de pago.</summary>
public enum AccountType
{
    CuentaCorriente,
    Ahorro,
    Otra
}
