using SpamRemovalAgent.Authentication.Models;

namespace SpamRemovalAgent.Authentication;

/// <summary>
/// Primary interface for OAuth 2.0 authentication with Microsoft Graph.
/// Supports both interactive and service principal authentication flows.
/// Thread-safe and suitable for singleton injection.
/// </summary>
public interface IOAuthAuthenticator
{
    /// <summary>
    /// Authenticates and obtains an access token for Microsoft Graph.
    /// Uses interactive flow (browser) for Windows local, service principal for cloud.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Valid access token for Graph API calls</returns>
    Task<string> AuthenticateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a valid access token, refreshing proactively if needed.
    /// This method handles token expiration and refresh automatically.
    /// </summary>
    /// <param name="forceRefresh">If true, forces token refresh even if not expired</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Valid access token for Graph API calls</returns>
    Task<string> GetValidAccessTokenAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all stored authentication credentials.
    /// Used for sign-out or when refresh token is permanently invalid.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    Task ClearCredentialsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if valid authentication credentials are currently stored.
    /// Does not validate token with server, only checks local storage.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>True if credentials exist and appear valid, false otherwise</returns>
    Task<bool> HasValidCredentialsAsync(CancellationToken cancellationToken = default);
}
