namespace SpamRemovalAgent.Authentication.Middleware;

using Microsoft.Extensions.Logging;
using SpamRemovalAgent.Authentication.Exceptions;
using SpamRemovalAgent.Authentication.TokenManagement;

/// <summary>
/// Middleware for proactive OAuth token refresh.
/// Monitors token expiration and refreshes tokens before they expire (5-minute buffer).
/// </summary>
public sealed class TokenRefreshMiddleware
{
    private readonly ITokenStore _tokenStore;
    private readonly IOAuthAuthenticator _authenticator;
    private readonly ILogger<TokenRefreshMiddleware> _logger;

    private const int TokenExpirationBufferMinutes = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRefreshMiddleware"/> class.
    /// </summary>
    /// <param name="tokenStore">Token storage for retrieving current tokens.</param>
    /// <param name="authenticator">OAuth authenticator for refreshing tokens.</param>
    /// <param name="logger">Logger for middleware operations.</param>
    public TokenRefreshMiddleware(
        ITokenStore tokenStore,
        IOAuthAuthenticator authenticator,
        ILogger<TokenRefreshMiddleware> logger)
    {
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a valid access token, proactively refreshing if expiring within the buffer period.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A valid access token for Microsoft Graph API calls.</returns>
    /// <exception cref="AuthenticationRequiredException">Thrown when token refresh fails and re-authentication is required.</exception>
    public async Task<string> GetValidAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await _tokenStore.RetrieveTokenAsync(cancellationToken);

        // No token found - authenticate
        if (token == null)
        {
            _logger.LogInformation("No token found in storage. Triggering authentication.");
            return await _authenticator.AuthenticateAsync(cancellationToken);
        }

        // Check if token is expiring soon
        var bufferTime = TimeSpan.FromMinutes(TokenExpirationBufferMinutes);
        var isExpiringSoon = token.IsExpiringSoon(bufferTime);

        if (!isExpiringSoon)
        {
            _logger.LogDebug("Token is valid. Expires at: {ExpiresAt} UTC", token.ExpiresAt);
            return token.AccessToken;
        }

        // Token is expiring soon - proactively refresh
        _logger.LogInformation("Token is expiring soon (within {BufferMinutes} minutes). Initiating proactive refresh. Expires at: {ExpiresAt} UTC",
            TokenExpirationBufferMinutes, token.ExpiresAt);

        try
        {
            // Use authenticator's GetValidAccessTokenAsync which handles refresh logic
            return await _authenticator.GetValidAccessTokenAsync(forceRefresh: true, cancellationToken);
        }
        catch (AuthenticationRequiredException ex)
        {
            _logger.LogWarning(ex, "Token refresh requires re-authentication. Clearing credentials.");

            // Clear invalid credentials
            await _authenticator.ClearCredentialsAsync(cancellationToken);

            throw;
        }
        catch (TransientAuthFailureException ex)
        {
            _logger.LogWarning(ex, "Token refresh failed with transient error. Retrying may succeed.");

            throw;
        }
    }

    /// <summary>
    /// Checks if the current token needs proactive refresh based on expiration time.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>True if token needs refresh, false otherwise.</returns>
    public async Task<bool> NeedsRefreshAsync(CancellationToken cancellationToken = default)
    {
        var token = await _tokenStore.RetrieveTokenAsync(cancellationToken);

        if (token == null)
        {
            _logger.LogDebug("No token found - needs authentication.");
            return true;
        }

        var bufferTime = TimeSpan.FromMinutes(TokenExpirationBufferMinutes);
        var needsRefresh = token.IsExpiringSoon(bufferTime);

        _logger.LogDebug("Token refresh check: {NeedsRefresh}. Token expires at: {ExpiresAt} UTC",
            needsRefresh, token.ExpiresAt);

        return needsRefresh;
    }
}
