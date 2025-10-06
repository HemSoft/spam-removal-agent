namespace SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Represents the OAuth 2.0 authentication flow mode.
/// </summary>
public enum AuthenticationMode
{
    /// <summary>
    /// Interactive authorization code flow with PKCE (for local development).
    /// Requires user interaction via browser consent.
    /// Provides refresh token for long-lived access.
    /// </summary>
    Interactive = 1,

    /// <summary>
    /// Service principal authentication with client credentials (for cloud deployments).
    /// Non-interactive, uses client ID and client secret.
    /// No refresh token (must re-authenticate when access token expires).
    /// </summary>
    ServicePrincipal = 2
}
