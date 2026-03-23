namespace DgiiSaas.Domain.Enums;

/// <summary>
/// Máquina de estados del documento electrónico.
/// </summary>
public enum DocumentStatus
{
    Draft = 0,
    ReceivedFromClient = 1,
    ValidationFailed = 2,
    Validated = 3,
    SigningFailed = 4,
    Signed = 5,
    AuthenticationFailed = 6,
    Authenticated = 7,
    SubmittedToDGII = 8,
    TrackIdReceived = 9,
    InProcess = 10,
    Accepted = 11,
    AcceptedWithConditions = 12,
    Rejected = 13,
    DeliveryPending = 14,
    DeliveredToReceiver = 15,
    CancelRequested = 16,
    Cancelled = 17,
    InContingency = 18,
    ReconciliationPending = 19,
    SubmissionFailed = 20
}
