namespace SpamRemovalAgent.Authentication.Flows
{
    using SpamRemovalAgent.Authentication.Models;

    /// <summary>
    /// Defines the contract for OAuth 2.0 authentication flows.
    /// Implementations provide environment-specific authentication mechanisms (Interactive, Service Principal).
    /// </summary>
    public interface IAuthFlow
    {
        /// <summary>
        /// Authenticates the user or application and obtains an OAuth token.
        /// </summary>
        /// <param name="config">The OAuth configuration containing client credentials and settings.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the authentication operation.</param>
        /// <returns>An OAuth token containing access token, refresh token, and expiration details.</returns>
        /// <exception cref="PermanentAuthFailureException">Thrown when authentication fails due to invalid configuration or credentials.</exception>
        /// <exception cref="TransientAuthFailureException">Thrown when authentication fails due to temporary issues (network, service availability).</exception>
        Task<OAuthToken> AuthenticateAsync(OAuthConfiguration config, CancellationToken cancellationToken);

        /// <summary>
        /// Refreshes an expired or expiring OAuth token using the refresh token.
        /// </summary>
        /// <param name="token">The current OAuth token containing a valid refresh token.</param>
        /// <param name="config">The OAuth configuration containing client credentials and settings.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the refresh operation.</param>
        /// <returns>A new OAuth token with updated access token and expiration.</returns>
        /// <exception cref="PermanentAuthFailureException">Thrown when refresh token is invalid or revoked.</exception>
        /// <exception cref="TransientAuthFailureException">Thrown when refresh fails due to temporary issues.</exception>
        /// <exception cref="AuthenticationRequiredException">Thrown when refresh token is expired and re-authentication is required.</exception>
        Task<OAuthToken> RefreshTokenAsync(OAuthToken token, OAuthConfiguration config, CancellationToken cancellationToken);
    }
}
