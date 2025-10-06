namespace SpamRemovalAgent.Authentication.Flows
{
    using Azure.Core;
    using Azure.Identity;
    using Microsoft.Extensions.Logging;
    using SpamRemovalAgent.Authentication.Exceptions;
    using SpamRemovalAgent.Authentication.Models;

    /// <summary>
    /// Implements OAuth 2.0 client credentials flow using service principal (application) identity.
    /// This flow uses client secret for authentication and is suitable for Azure cloud and GitHub Actions deployments.
    /// </summary>
    public sealed class ServicePrincipalAuthFlow : IAuthFlow
    {
        private readonly ILogger<ServicePrincipalAuthFlow> _logger;
        private const string GraphScope = "https://graph.microsoft.com/.default";

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicePrincipalAuthFlow"/> class.
        /// </summary>
        /// <param name="logger">Logger for authentication events and diagnostics.</param>
        public ServicePrincipalAuthFlow(ILogger<ServicePrincipalAuthFlow> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<OAuthToken> AuthenticateAsync(OAuthConfiguration config, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(config);

            if (string.IsNullOrWhiteSpace(config.ClientSecret))
            {
                throw new PermanentAuthFailureException(
                    "Client secret is required for service principal authentication.",
                    "missing_client_secret",
                    Guid.NewGuid().ToString());
            }

            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("Starting service principal authentication flow. TenantId: {TenantId}, ClientId: {ClientId}, CorrelationId: {CorrelationId}",
                config.TenantId, config.ClientId, correlationId);

            try
            {
                var credential = new ClientSecretCredential(
                    tenantId: config.TenantId,
                    clientId: config.ClientId,
                    clientSecret: config.ClientSecret,
                    options: new ClientSecretCredentialOptions
                    {
                        AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                        Retry =
                        {
                            MaxRetries = 0 // We handle retries in RetryPolicyMiddleware
                        }
                    });

                var tokenRequestContext = new TokenRequestContext(
                    scopes: new[] { GraphScope },
                    parentRequestId: correlationId);

                AccessToken accessToken;
                try
                {
                    accessToken = await credential.GetTokenAsync(tokenRequestContext, cancellationToken);
                }
                catch (AuthenticationFailedException ex) when (IsTransientError(ex))
                {
                    _logger.LogWarning(ex, "Transient authentication failure during service principal authentication. CorrelationId: {CorrelationId}",
                        correlationId);
                    throw new TransientAuthFailureException(
                        $"Service principal authentication temporarily unavailable: {ex.Message}",
                        ex,
                        1);
                }
                catch (AuthenticationFailedException ex)
                {
                    _logger.LogError(ex, "Service principal authentication failed. CorrelationId: {CorrelationId}", correlationId);
                    throw new PermanentAuthFailureException(
                        $"Service principal authentication failed: {ex.Message}",
                        "authentication_failed",
                        ex,
                        correlationId);
                }

                var token = new OAuthToken
                {
                    AccessToken = accessToken.Token,
                    RefreshToken = string.Empty, // Service principal flow does not provide refresh tokens
                    ExpiresAt = accessToken.ExpiresOn.UtcDateTime,
                    Scope = GraphScope,
                    TokenType = "Bearer",
                    CorrelationId = correlationId
                };

                _logger.LogInformation("Service principal authentication succeeded. Token expires at: {ExpiresAt} UTC. CorrelationId: {CorrelationId}",
                    token.ExpiresAt, correlationId);

                return token;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Service principal authentication was cancelled. CorrelationId: {CorrelationId}", correlationId);
                throw;
            }
            catch (Exception ex) when (ex is not PermanentAuthFailureException and not TransientAuthFailureException)
            {
                _logger.LogError(ex, "Unexpected error during service principal authentication. CorrelationId: {CorrelationId}", correlationId);
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
            _logger.LogInformation("Service principal token refresh (re-authentication). CorrelationId: {CorrelationId}", correlationId);

            // Service principal flow does not use refresh tokens - we simply re-authenticate to get a new access token
            // This is the standard behavior for client credentials flow
            try
            {
                var newToken = await AuthenticateAsync(config, cancellationToken);

                // Preserve the original correlation ID for tracking
                return newToken with { CorrelationId = correlationId };
            }
            catch (PermanentAuthFailureException ex)
            {
                _logger.LogError(ex, "Service principal token refresh failed permanently. CorrelationId: {CorrelationId}", correlationId);
                throw new SpamRemovalAgent.Authentication.Exceptions.AuthenticationRequiredException(
                    $"Service principal credentials are invalid or revoked: {ex.Message}",
                    ex);
            }
            catch (TransientAuthFailureException)
            {
                // Allow transient failures to bubble up for retry logic
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during service principal token refresh. CorrelationId: {CorrelationId}", correlationId);
                throw new SpamRemovalAgent.Authentication.Exceptions.AuthenticationRequiredException(
                    $"Unexpected token refresh error: {ex.Message}",
                    ex);
            }
        }

        private static bool IsTransientError(AuthenticationFailedException ex)
        {
            // Check for known transient error indicators
            var message = ex.Message.ToLowerInvariant();
            return message.Contains("timeout") ||
                   message.Contains("network") ||
                   message.Contains("temporarily unavailable") ||
                   message.Contains("service unavailable") ||
                   message.Contains("429") ||
                   message.Contains("503") ||
                   message.Contains("504");
        }

        private static string GetRecoveryInstructions(AuthenticationFailedException ex)
        {
            var message = ex.Message.ToLowerInvariant();

            if (message.Contains("invalid_client") || message.Contains("unauthorized_client"))
            {
                return "Verify the Client ID and Client Secret are correct. Check that the service principal exists in Azure AD.";
            }

            if (message.Contains("invalid_scope"))
            {
                return "Verify the service principal has been granted the 'Mail.ReadWrite' application permission in the Azure AD app registration.";
            }

            if (message.Contains("credential"))
            {
                return "The client secret may be expired or incorrect. Generate a new client secret in Azure AD and update the configuration.";
            }

            return "Check the service principal configuration in Azure AD. Verify the tenant ID, client ID, and client secret are correct.";
        }
    }
}
