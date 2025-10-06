using SpamRemovalAgent.Authentication.Models;

namespace SpamRemovalAgent.Tests.Utilities;

/// <summary>
/// Factory for creating test OAuth tokens with realistic values.
/// </summary>
public static class TestOAuthTokenFactory
{
    /// <summary>
    /// Creates a valid OAuth token for testing purposes.
    /// </summary>
    public static OAuthToken CreateValidToken(
        TimeSpan? expiresIn = null,
        string? scope = null,
        bool includeRefreshToken = true)
    {
        var expiration = DateTimeOffset.UtcNow.Add(expiresIn ?? TimeSpan.FromHours(1));

        return new OAuthToken
        {
            AccessToken = GenerateMockJwt(),
            RefreshToken = includeRefreshToken ? GenerateMockRefreshToken() : null,
            ExpiresAt = expiration,
            Scope = scope ?? "Mail.ReadWrite offline_access",
            TokenType = "Bearer",
            CorrelationId = Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    /// Creates an expired OAuth token for testing token refresh scenarios.
    /// </summary>
    public static OAuthToken CreateExpiredToken()
    {
        return CreateValidToken(expiresIn: TimeSpan.FromMinutes(-10));
    }

    /// <summary>
    /// Creates a token that is expiring soon (within 5 minutes).
    /// </summary>
    public static OAuthToken CreateExpiringSoonToken()
    {
        return CreateValidToken(expiresIn: TimeSpan.FromMinutes(3));
    }

    /// <summary>
    /// Generates a mock JWT token (3 base64-encoded segments).
    /// </summary>
    private static string GenerateMockJwt()
    {
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"typ\":\"JWT\",\"alg\":\"RS256\"}"));
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"aud\":\"https://graph.microsoft.com\",\"iss\":\"https://sts.windows.net/\"}"));
        var signature = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        return $"{header}.{payload}.{signature}";
    }

    /// <summary>
    /// Generates a mock refresh token (opaque string).
    /// </summary>
    private static string GenerateMockRefreshToken()
    {
        return Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
    }
}
