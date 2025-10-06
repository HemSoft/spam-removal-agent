namespace SpamRemovalAgent.Authentication.Flows
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Client;
    using SpamRemovalAgent.Authentication.Exceptions;
    using SpamRemovalAgent.Authentication.Models;

    /// <summary>
    /// Implements interactive OAuth 2.0 authentication flow using authorization code with PKCE.
    /// This flow opens a system browser for user consent and is suitable for Windows local development.
    /// </summary>
    public sealed class InteractiveAuthFlow : IAuthFlow
    {
        private readonly ILogger<InteractiveAuthFlow> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveAuthFlow"/> class.
        /// </summary>
        /// <param name="logger">Logger for authentication events and diagnostics.</param>
        public InteractiveAuthFlow(ILogger<InteractiveAuthFlow> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<OAuthToken> AuthenticateAsync(OAuthConfiguration config, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(config);

            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("Starting interactive authentication flow. CorrelationId: {CorrelationId}", correlationId);

            try
            {
                var app = PublicClientApplicationBuilder
                    .Create(config.ClientId)
                    .WithAuthority(AzureCloudInstance.AzurePublic, config.TenantId)
                    .WithRedirectUri(config.RedirectUri)
                    .WithLogging((level, message, containsPii) =>
                    {
                        _logger.LogDebug("MSAL: {Message}", message);
                    }, Microsoft.Identity.Client.LogLevel.Info, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                    .Build();

                var scopes = config.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                Microsoft.Identity.Client.AuthenticationResult result;
                try
                {
                    result = await app.AcquireTokenInteractive(scopes)
                        .WithPrompt(Prompt.SelectAccount)
                        .WithCorrelationId(Guid.Parse(correlationId))
                        .ExecuteAsync(cancellationToken);
                }
                catch (MsalServiceException ex) when (ex.StatusCode is >= 500 and < 600)
                {
                    _logger.LogWarning(ex, "Transient authentication failure (HTTP {StatusCode}). CorrelationId: {CorrelationId}",
                        ex.StatusCode, correlationId);
                    throw new TransientAuthFailureException(
                        $"Microsoft Identity service is temporarily unavailable (HTTP {ex.StatusCode})",
                        ex,
                        1);
                }
                catch (MsalClientException ex) when (ex.ErrorCode == MsalError.AuthenticationCanceledError)
                {
                    _logger.LogWarning("User cancelled interactive authentication. CorrelationId: {CorrelationId}", correlationId);
                    throw new PermanentAuthFailureException(
                        "Authentication was cancelled by the user. Please try again and complete the sign-in process.",
                        MsalError.AuthenticationCanceledError,
                        ex,
                        correlationId);
                }
                catch (MsalException ex)
                {
                    _logger.LogError(ex, "Interactive authentication failed with error code: {ErrorCode}. CorrelationId: {CorrelationId}",
                        ex.ErrorCode, correlationId);
                    throw new PermanentAuthFailureException(
                        $"Authentication failed: {ex.Message}",
                        ex.ErrorCode,
                        ex,
                        correlationId);
                }

                var token = new OAuthToken
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.Account?.HomeAccountId?.Identifier != null ? "refresh_token_placeholder" : string.Empty,
                    ExpiresAt = result.ExpiresOn.UtcDateTime,
                    Scope = string.Join(" ", result.Scopes),
                    TokenType = "Bearer",
                    CorrelationId = correlationId
                };

                _logger.LogInformation("Interactive authentication succeeded. Token expires at: {ExpiresAt} UTC. CorrelationId: {CorrelationId}",
                    token.ExpiresAt, correlationId);

                return token;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Interactive authentication was cancelled. CorrelationId: {CorrelationId}", correlationId);
                throw;
            }
            catch (Exception ex) when (ex is not PermanentAuthFailureException and not TransientAuthFailureException)
            {
                _logger.LogError(ex, "Unexpected error during interactive authentication. CorrelationId: {CorrelationId}", correlationId);
                throw new PermanentAuthFailureException(
                    $"Unexpected authentication error: {ex.Message}",
                    "unexpected_error",
                    ex,
                    correlationId);
            }
        }

        /// <inheritdoc />
        public async Task<OAuthToken> RefreshTokenAsync(OAuthToken token, OAuthConfiguration config, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(token);
            ArgumentNullException.ThrowIfNull(config);

            var correlationId = token.CorrelationId ?? Guid.NewGuid().ToString();
            _logger.LogInformation("Starting silent token refresh. CorrelationId: {CorrelationId}", correlationId);

            try
            {
                var app = PublicClientApplicationBuilder
                    .Create(config.ClientId)
                    .WithAuthority(AzureCloudInstance.AzurePublic, config.TenantId)
                    .WithRedirectUri(config.RedirectUri)
                    .WithLogging((level, message, containsPii) =>
                    {
                        _logger.LogDebug("MSAL: {Message}", message);
                    }, Microsoft.Identity.Client.LogLevel.Info, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                    .Build();

                var accounts = await app.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();

                if (firstAccount == null)
                {
                    _logger.LogWarning("No cached accounts found for silent refresh. Re-authentication required. CorrelationId: {CorrelationId}",
                        correlationId);
                    throw new SpamRemovalAgent.Authentication.Exceptions.AuthenticationRequiredException(
                        "No cached account found. Please sign in again.");
                }

                var scopes = config.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                Microsoft.Identity.Client.AuthenticationResult result;
                try
                {
                    result = await app.AcquireTokenSilent(scopes, firstAccount)
                        .WithCorrelationId(Guid.Parse(correlationId))
                        .ExecuteAsync(cancellationToken);
                }
                catch (MsalUiRequiredException ex)
                {
                    _logger.LogWarning(ex, "Silent token refresh requires user interaction. CorrelationId: {CorrelationId}", correlationId);
                    throw new SpamRemovalAgent.Authentication.Exceptions.AuthenticationRequiredException(
                        "Token refresh requires user interaction. Please sign in again.",
                        ex);
                }
                catch (MsalServiceException ex) when (ex.StatusCode is >= 500 and < 600)
                {
                    _logger.LogWarning(ex, "Transient token refresh failure (HTTP {StatusCode}). CorrelationId: {CorrelationId}",
                        ex.StatusCode, correlationId);
                    throw new TransientAuthFailureException(
                        $"Token refresh temporarily unavailable (HTTP {ex.StatusCode})",
                        ex,
                        1);
                }
                catch (MsalException ex)
                {
                    _logger.LogError(ex, "Silent token refresh failed with error code: {ErrorCode}. CorrelationId: {CorrelationId}",
                        ex.ErrorCode, correlationId);
                    throw new SpamRemovalAgent.Authentication.Exceptions.AuthenticationRequiredException(
                        $"Token refresh failed: {ex.Message}",
                        ex);
                }

                var refreshedToken = new OAuthToken
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = token.RefreshToken, // Preserve original refresh token identifier
                    ExpiresAt = result.ExpiresOn.UtcDateTime,
                    Scope = string.Join(" ", result.Scopes),
                    TokenType = "Bearer",
                    CorrelationId = correlationId
                };

                _logger.LogInformation("Silent token refresh succeeded. New token expires at: {ExpiresAt} UTC. CorrelationId: {CorrelationId}",
                    refreshedToken.ExpiresAt, correlationId);

                return refreshedToken;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Token refresh was cancelled. CorrelationId: {CorrelationId}", correlationId);
                throw;
            }
            catch (Exception ex) when (ex is not SpamRemovalAgent.Authentication.Exceptions.AuthenticationRequiredException and not TransientAuthFailureException)
            {
                _logger.LogError(ex, "Unexpected error during token refresh. CorrelationId: {CorrelationId}", correlationId);
                throw new SpamRemovalAgent.Authentication.Exceptions.AuthenticationRequiredException(
                    $"Unexpected token refresh error: {ex.Message}",
                    ex);
            }
        }

        private static string GetRecoveryInstructions(string errorCode)
        {
            return errorCode switch
            {
                MsalError.InvalidClient => "Verify the Client ID in your configuration matches the Azure AD app registration.",
                "invalid_scope" => "Verify the requested scopes (Mail.ReadWrite) are granted in the Azure AD app registration.",
                MsalError.UnauthorizedClient => "Ensure the Azure AD app registration is configured for public client flows (mobile and desktop applications).",
                MsalError.AccessDenied => "User or administrator denied consent. Grant necessary permissions in Azure AD.",
                _ => "Check your Azure AD app registration configuration and ensure the application has the required API permissions."
            };
        }
    }
}
