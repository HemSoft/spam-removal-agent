namespace SpamRemovalAgent.Authentication.Exceptions;

/// <summary>
/// Exception thrown when authentication fails permanently and should not be retried.
/// Examples: invalid_client, consent_required, access_denied.
/// </summary>
public class PermanentAuthFailureException : Exception
{
    /// <summary>
    /// OAuth 2.0 error code (e.g., "invalid_grant", "consent_required").
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Azure AD correlation ID for troubleshooting.
    /// </summary>
    public string? CorrelationId { get; }

    public PermanentAuthFailureException(string message, string errorCode, string? correlationId = null)
        : base(message)
    {
        ErrorCode = errorCode;
        CorrelationId = correlationId;
    }

    public PermanentAuthFailureException(string message, string errorCode, Exception innerException, string? correlationId = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        CorrelationId = correlationId;
    }
}
