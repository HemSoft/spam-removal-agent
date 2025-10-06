namespace SpamRemovalAgent.Authentication.TokenManagement
{
    using System.Globalization;
    using Microsoft.Extensions.Logging;
    using SpamRemovalAgent.Authentication.Exceptions;
    using SpamRemovalAgent.Authentication.Models;

    /// <summary>
    /// Token storage implementation for GitHub Actions using environment variables (read-only).
    /// This store reads OAuth tokens from GitHub Secrets exposed as environment variables.
    /// Token storage is not supported - use GitHub CLI to update secrets in the workflow.
    /// </summary>
    public sealed class GitHubSecretsTokenStore : ITokenStore
    {
        private readonly ILogger<GitHubSecretsTokenStore> _logger;
        private const string AccessTokenEnvVar = "OAUTH_ACCESS_TOKEN";
        private const string RefreshTokenEnvVar = "OAUTH_REFRESH_TOKEN";
        private const string ExpiryEnvVar = "OAUTH_EXPIRY";
        private const string GitHubActionsEnvVar = "GITHUB_ACTIONS";

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubSecretsTokenStore"/> class.
        /// </summary>
        /// <param name="logger">Logger for token storage operations.</param>
        public GitHubSecretsTokenStore(ILogger<GitHubSecretsTokenStore> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task<bool> IsAvailableAsync()
        {
            // Check if running in GitHub Actions
            var isGitHubActions = System.Environment.GetEnvironmentVariable(GitHubActionsEnvVar);
            if (string.IsNullOrWhiteSpace(isGitHubActions) || !isGitHubActions.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Not running in GitHub Actions environment.");
                return Task.FromResult(false);
            }

            // Check if required environment variables exist
            var hasAccessToken = !string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable(AccessTokenEnvVar));
            var hasRefreshToken = !string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable(RefreshTokenEnvVar));
            var hasExpiry = !string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable(ExpiryEnvVar));

            var isAvailable = hasAccessToken && hasRefreshToken && hasExpiry;

            if (isAvailable)
            {
                _logger.LogDebug("GitHub Secrets token store is available. All required environment variables are set.");
            }
            else
            {
                _logger.LogWarning("GitHub Secrets token store is not available. Missing environment variables: {MissingVars}",
                    string.Join(", ", GetMissingEnvironmentVariables(hasAccessToken, hasRefreshToken, hasExpiry)));
            }

            return Task.FromResult(isAvailable);
        }

        /// <inheritdoc />
        public Task StoreTokenAsync(OAuthToken token, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(token);

            _logger.LogError("Token storage is not supported in GitHub Actions. Use GitHub CLI to update secrets.");

            throw new NotSupportedException(
                "Token storage is not supported in GitHub Actions environment. " +
                "To update OAuth tokens, use GitHub CLI: " +
                "'gh secret set OAUTH_ACCESS_TOKEN --body \"<access-token>\"', " +
                "'gh secret set OAUTH_REFRESH_TOKEN --body \"<refresh-token>\"', " +
                "'gh secret set OAUTH_EXPIRY --body \"<expiry-iso8601>\"'");
        }

        /// <inheritdoc />
        public async Task<OAuthToken?> RetrieveTokenAsync(CancellationToken cancellationToken = default)
        {
            if (!await IsAvailableAsync())
            {
                throw new TokenStoreUnavailableException(
                    "GitHub Secrets token store is not available. Verify GITHUB_ACTIONS=true and OAuth secrets are configured.",
                    "GitHubSecrets");
            }

            try
            {
                var accessToken = System.Environment.GetEnvironmentVariable(AccessTokenEnvVar);
                var refreshToken = System.Environment.GetEnvironmentVariable(RefreshTokenEnvVar);
                var expiryString = System.Environment.GetEnvironmentVariable(ExpiryEnvVar);

                if (string.IsNullOrWhiteSpace(accessToken) ||
                    string.IsNullOrWhiteSpace(refreshToken) ||
                    string.IsNullOrWhiteSpace(expiryString))
                {
                    _logger.LogWarning("One or more OAuth environment variables are empty.");
                    return null;
                }

                if (!DateTime.TryParse(expiryString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiresAt))
                {
                    _logger.LogError("Failed to parse OAUTH_EXPIRY environment variable: {ExpiryString}", expiryString);
                    return null;
                }

                var token = new OAuthToken
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt.ToUniversalTime(),
                    Scope = "Mail.ReadWrite offline_access", // Default scope for this application
                    TokenType = "Bearer",
                    CorrelationId = System.Environment.GetEnvironmentVariable("GITHUB_RUN_ID") // Use GitHub run ID as correlation
                };

                _logger.LogInformation("OAuth token retrieved successfully from GitHub Secrets. Expires at: {ExpiresAt} UTC. CorrelationId: {CorrelationId}",
                    token.ExpiresAt, token.CorrelationId);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving token from GitHub Secrets.");
                throw new TokenStoreUnavailableException(
                    $"Unexpected error retrieving token from GitHub Secrets: {ex.Message}",
                    "GitHubSecrets",
                    ex);
            }
        }

        /// <inheritdoc />
        public Task ClearTokenAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Token clearing is not supported in GitHub Actions. GitHub Secrets cannot be deleted from the workflow.");

            throw new NotSupportedException(
                "Token clearing is not supported in GitHub Actions environment. " +
                "To remove OAuth tokens, delete the GitHub Secrets manually: " +
                "'gh secret delete OAUTH_ACCESS_TOKEN', " +
                "'gh secret delete OAUTH_REFRESH_TOKEN', " +
                "'gh secret delete OAUTH_EXPIRY'");
        }

        private static IEnumerable<string> GetMissingEnvironmentVariables(bool hasAccessToken, bool hasRefreshToken, bool hasExpiry)
        {
            if (!hasAccessToken)
                yield return AccessTokenEnvVar;
            if (!hasRefreshToken)
                yield return RefreshTokenEnvVar;
            if (!hasExpiry)
                yield return ExpiryEnvVar;
        }
    }
}
