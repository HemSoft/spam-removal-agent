using System.Text.Json.Serialization;

namespace SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Represents an OAuth 2.0 token set including access token, refresh token, and expiration information.
/// </summary>
public sealed record OAuthToken
{
    /// <summary>
    /// The access token used to authenticate Graph API requests.
    /// Format: JWT (3 base64-encoded segments separated by dots).
    /// Lifetime: 60-90 minutes (determined by Microsoft Entra ID).
    /// </summary>
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    /// <summary>
    /// The refresh token used to obtain new access tokens when the current one expires.
    /// Format: Opaque string (not JWT).
    /// Lifetime: 90 days (rolling window) for standard apps, 24 hours for SPA apps.
    /// May be null for client credentials flow (no refresh token issued).
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    /// <summary>
    /// The absolute UTC timestamp when the access token expires.
    /// System should proactively refresh 5 minutes before this time.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public required DateTimeOffset ExpiresAt { get; init; }

    /// <summary>
    /// The space-separated list of Microsoft Graph scopes granted by this token.
    /// Example: "Mail.ReadWrite offline_access User.Read"
    /// </summary>
    [JsonPropertyName("scope")]
    public required string Scope { get; init; }

    /// <summary>
    /// The type of token (always "Bearer" for Microsoft identity platform).
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Optional: The Azure AD correlation ID from the token response.
    /// Used for troubleshooting and audit logging.
    /// </summary>
    [JsonPropertyName("correlation_id")]
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Checks if the access token is expired or expiring within the specified buffer time.
    /// </summary>
    /// <param name="bufferTime">Time buffer before actual expiration (default: 5 minutes)</param>
    /// <returns>True if token needs refresh, false otherwise</returns>
    public bool IsExpiringSoon(TimeSpan? bufferTime = null)
    {
        var buffer = bufferTime ?? TimeSpan.FromMinutes(5);
        return (ExpiresAt - DateTimeOffset.UtcNow) <= buffer;
    }

    /// <summary>
    /// Validates that required fields are populated and values are within expected constraints.
    /// </summary>
    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(AccessToken))
            errors.Add("AccessToken is required");

        if (!AccessToken.Contains('.'))
            errors.Add("AccessToken must be a valid JWT format");

        if (ExpiresAt <= DateTimeOffset.UtcNow)
            errors.Add("Token has already expired");

        if (string.IsNullOrWhiteSpace(Scope))
            errors.Add("Scope is required");

        if (!Scope.Contains("Mail.ReadWrite", StringComparison.OrdinalIgnoreCase))
            errors.Add("Scope must include Mail.ReadWrite permission");

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
