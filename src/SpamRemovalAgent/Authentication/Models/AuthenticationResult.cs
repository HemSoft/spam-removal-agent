namespace SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Result of an OAuth 2.0 authentication operation.
/// </summary>
public sealed record AuthenticationResult
{
    /// <summary>
    /// Indicates whether authentication was successful.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// The obtained access token (only populated if IsSuccess = true).
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// The complete OAuth token set (only populated if IsSuccess = true).
    /// </summary>
    public OAuthToken? Token { get; init; }

    /// <summary>
    /// Error code if authentication failed (e.g., "invalid_grant", "consent_required").
    /// Maps to standard OAuth 2.0 error codes or custom application error codes.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Human-readable error message if authentication failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Azure AD correlation ID for troubleshooting.
    /// Present in both success and failure scenarios.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Time taken to complete the authentication operation.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Indicates whether the failure is permanent (should not retry).
    /// Examples: consent_required, invalid_client, access_denied.
    /// </summary>
    public bool IsPermanentFailure { get; init; }

    /// <summary>
    /// Creates a successful authentication result.
    /// </summary>
    public static AuthenticationResult Success(OAuthToken token, TimeSpan duration, string? correlationId = null)
    {
        return new AuthenticationResult
        {
            IsSuccess = true,
            AccessToken = token.AccessToken,
            Token = token,
            Duration = duration,
            CorrelationId = correlationId ?? token.CorrelationId
        };
    }

    /// <summary>
    /// Creates a failed authentication result with retry eligibility.
    /// </summary>
    public static AuthenticationResult Failure(
        string errorCode,
        string errorMessage,
        TimeSpan duration,
        bool isPermanent = false,
        string? correlationId = null)
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Duration = duration,
            IsPermanentFailure = isPermanent,
            CorrelationId = correlationId
        };
    }
}
