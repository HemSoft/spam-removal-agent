namespace SpamRemovalAgent.Authentication.TokenManagement
{
    using System.Text.Json;
    using Azure;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.Extensions.Logging;
    using SpamRemovalAgent.Authentication.Exceptions;
    using SpamRemovalAgent.Authentication.Models;

    /// <summary>
    /// Token storage implementation using Azure Key Vault with managed identity authentication.
    /// This store is suitable for Azure cloud deployments where Key Vault access is available.
    /// </summary>
    public sealed class AzureKeyVaultTokenStore : ITokenStore
    {
        private readonly ILogger<AzureKeyVaultTokenStore> _logger;
        private readonly string? _keyVaultUri;
        private const string SecretName = "oauth-token";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureKeyVaultTokenStore"/> class.
        /// </summary>
        /// <param name="logger">Logger for token storage operations.</param>
        /// <param name="keyVaultUri">The Azure Key Vault URI. If null, reads from AZURE_KEY_VAULT_URI environment variable.</param>
        public AzureKeyVaultTokenStore(ILogger<AzureKeyVaultTokenStore> logger, string? keyVaultUri = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyVaultUri = keyVaultUri ?? System.Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI");
        }

        /// <inheritdoc />
        public async Task<bool> IsAvailableAsync()
        {
            if (string.IsNullOrWhiteSpace(_keyVaultUri))
            {
                _logger.LogWarning("Azure Key Vault URI not configured. Set AZURE_KEY_VAULT_URI environment variable.");
                return false;
            }

            try
            {
                var client = CreateSecretClient();

                // Test connectivity by attempting to get a non-existent secret
                // This validates that the Key Vault exists and we have access
                await client.GetSecretAsync("connectivity-test");
                return false;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // 404 means Key Vault is accessible but secret doesn't exist - this is expected and OK
                _logger.LogDebug("Azure Key Vault is accessible. URI: {KeyVaultUri}", _keyVaultUri);
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 403)
            {
                _logger.LogWarning("Azure Key Vault access denied. Verify managed identity has 'Get' and 'Set' permissions. URI: {KeyVaultUri}", _keyVaultUri);
                return false;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogWarning(ex, "Azure Key Vault connectivity test failed with status {StatusCode}. URI: {KeyVaultUri}",
                    ex.Status, _keyVaultUri);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Azure Key Vault availability check failed. URI: {KeyVaultUri}", _keyVaultUri);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task StoreTokenAsync(OAuthToken token, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(token);

            if (!await IsAvailableAsync())
            {
                throw new TokenStoreUnavailableException(
                    "Azure Key Vault is not available. Check Key Vault URI and managed identity permissions.",
                    "AzureKeyVault");
            }

            try
            {
                var client = CreateSecretClient();
                var tokenJson = JsonSerializer.Serialize(token);

                var secret = new KeyVaultSecret(SecretName, tokenJson)
                {
                    Properties =
                    {
                        ExpiresOn = token.ExpiresAt,
                        ContentType = "application/json"
                    }
                };

                if (!string.IsNullOrWhiteSpace(token.CorrelationId))
                {
                    secret.Properties.Tags["CorrelationId"] = token.CorrelationId;
                }

                await client.SetSecretAsync(secret, cancellationToken);

                _logger.LogInformation("OAuth token stored successfully in Azure Key Vault. Expires at: {ExpiresAt} UTC. CorrelationId: {CorrelationId}",
                    token.ExpiresAt, token.CorrelationId);
            }
            catch (RequestFailedException ex) when (ex.Status == 403)
            {
                _logger.LogError(ex, "Access denied when storing token in Azure Key Vault. Verify managed identity has 'Set' permission.");
                throw new TokenStoreUnavailableException(
                    "Azure Key Vault access denied. Verify managed identity has 'Set' permission.",
                    "AzureKeyVault",
                    ex);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to store token in Azure Key Vault with status {StatusCode}.", ex.Status);
                throw new TokenStoreUnavailableException(
                    $"Failed to store token in Azure Key Vault: {ex.Message}",
                    "AzureKeyVault",
                    ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error storing token in Azure Key Vault.");
                throw new TokenStoreUnavailableException(
                    $"Unexpected error storing token: {ex.Message}",
                    "AzureKeyVault",
                    ex);
            }
        }

        /// <inheritdoc />
        public async Task<OAuthToken?> RetrieveTokenAsync(CancellationToken cancellationToken = default)
        {
            if (!await IsAvailableAsync())
            {
                throw new TokenStoreUnavailableException(
                    "Azure Key Vault is not available. Check Key Vault URI and managed identity permissions.",
                    "AzureKeyVault");
            }

            try
            {
                var client = CreateSecretClient();
                var secretResponse = await client.GetSecretAsync(SecretName, cancellationToken: cancellationToken);

                var tokenJson = secretResponse.Value.Value;
                var token = JsonSerializer.Deserialize<OAuthToken>(tokenJson);

                if (token != null)
                {
                    _logger.LogInformation("OAuth token retrieved successfully from Azure Key Vault. Expires at: {ExpiresAt} UTC. CorrelationId: {CorrelationId}",
                        token.ExpiresAt, token.CorrelationId);
                }

                return token;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogInformation("No OAuth token found in Azure Key Vault.");
                return null;
            }
            catch (RequestFailedException ex) when (ex.Status == 403)
            {
                _logger.LogError(ex, "Access denied when retrieving token from Azure Key Vault. Verify managed identity has 'Get' permission.");
                throw new TokenStoreUnavailableException(
                    "Azure Key Vault access denied. Verify managed identity has 'Get' permission.",
                    "AzureKeyVault",
                    ex);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to retrieve token from Azure Key Vault with status {StatusCode}.", ex.Status);
                throw new TokenStoreUnavailableException(
                    $"Failed to retrieve token from Azure Key Vault: {ex.Message}",
                    "AzureKeyVault",
                    ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize token from Azure Key Vault. Token may be corrupted.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving token from Azure Key Vault.");
                throw new TokenStoreUnavailableException(
                    $"Unexpected error retrieving token: {ex.Message}",
                    "AzureKeyVault",
                    ex);
            }
        }

        /// <inheritdoc />
        public async Task ClearTokenAsync(CancellationToken cancellationToken = default)
        {
            if (!await IsAvailableAsync())
            {
                throw new TokenStoreUnavailableException(
                    "Azure Key Vault is not available. Check Key Vault URI and managed identity permissions.",
                    "AzureKeyVault");
            }

            try
            {
                var client = CreateSecretClient();

                // Start the delete operation
                var deleteOperation = await client.StartDeleteSecretAsync(SecretName, cancellationToken);

                // Wait for the delete to complete
                await deleteOperation.WaitForCompletionAsync(cancellationToken);

                _logger.LogInformation("OAuth token cleared successfully from Azure Key Vault.");
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Secret doesn't exist - this is OK, nothing to clear
                _logger.LogDebug("No OAuth token to clear from Azure Key Vault (secret does not exist).");
            }
            catch (RequestFailedException ex) when (ex.Status == 403)
            {
                _logger.LogError(ex, "Access denied when clearing token from Azure Key Vault. Verify managed identity has 'Delete' permission.");
                throw new TokenStoreUnavailableException(
                    "Azure Key Vault access denied. Verify managed identity has 'Delete' permission.",
                    "AzureKeyVault",
                    ex);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to clear token from Azure Key Vault with status {StatusCode}.", ex.Status);
                throw new TokenStoreUnavailableException(
                    $"Failed to clear token from Azure Key Vault: {ex.Message}",
                    "AzureKeyVault",
                    ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error clearing token from Azure Key Vault.");
                throw new TokenStoreUnavailableException(
                    $"Unexpected error clearing token: {ex.Message}",
                    "AzureKeyVault",
                    ex);
            }
        }

        private SecretClient CreateSecretClient()
        {
            if (string.IsNullOrWhiteSpace(_keyVaultUri))
            {
                throw new InvalidOperationException("Azure Key Vault URI is not configured.");
            }

            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = false,
                ExcludeAzureCliCredential = false,
                ExcludeManagedIdentityCredential = false,
                ExcludeVisualStudioCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeInteractiveBrowserCredential = true,
                Retry =
                {
                    MaxRetries = 0 // We handle retries in RetryPolicyMiddleware
                }
            });

            return new SecretClient(new Uri(_keyVaultUri), credential);
        }
    }
}
