namespace SpamRemovalAgent.Authentication.TokenManagement
{
    using System.Runtime.Versioning;
    using Microsoft.Extensions.Logging;
    using SpamRemovalAgent.Authentication.Models;

    /// <summary>
    /// Factory for creating environment-specific token stores.
    /// Selects the appropriate storage mechanism based on the deployment environment.
    /// </summary>
    public static class TokenStoreFactory
    {
        /// <summary>
        /// Creates a token store appropriate for the specified deployment environment.
        /// </summary>
        /// <param name="environment">The deployment environment (Windows local, Azure, GitHub Actions).</param>
        /// <param name="loggerFactory">Logger factory for creating store-specific loggers.</param>
        /// <param name="keyVaultUri">Optional Azure Key Vault URI. If null, reads from AZURE_KEY_VAULT_URI environment variable.</param>
        /// <returns>An <see cref="ITokenStore"/> implementation suitable for the environment.</returns>
        /// <exception cref="NotSupportedException">Thrown when the environment is Unknown or unsupported.</exception>
        /// <exception cref="ArgumentNullException">Thrown when loggerFactory is null.</exception>
        public static ITokenStore Create(
            DeploymentEnvironment environment,
            ILoggerFactory loggerFactory,
            string? keyVaultUri = null)
        {
            ArgumentNullException.ThrowIfNull(loggerFactory);

            return environment switch
            {
#pragma warning disable CA1416 // Validate platform compatibility
                DeploymentEnvironment.WindowsLocal => new WindowsCredentialStore(),
#pragma warning restore CA1416 // Validate platform compatibility

                DeploymentEnvironment.Azure => new AzureKeyVaultTokenStore(
                    loggerFactory.CreateLogger<AzureKeyVaultTokenStore>(),
                    keyVaultUri),

                DeploymentEnvironment.GitHubActions => new GitHubSecretsTokenStore(
                    loggerFactory.CreateLogger<GitHubSecretsTokenStore>()),

                DeploymentEnvironment.Unknown => throw new NotSupportedException(
                    "Cannot create token store for unknown deployment environment. " +
                    "Verify environment detection or explicitly set deployment environment."),

                _ => throw new NotSupportedException(
                    $"Token store not implemented for deployment environment: {environment}")
            };
        }

        /// <summary>
        /// Creates a token store appropriate for the current deployment environment.
        /// Automatically detects the environment using <see cref="IDeploymentEnvironmentDetector"/>.
        /// </summary>
        /// <param name="environmentDetector">The environment detector to determine the current deployment environment.</param>
        /// <param name="loggerFactory">Logger factory for creating store-specific loggers.</param>
        /// <param name="keyVaultUri">Optional Azure Key Vault URI. If null, reads from AZURE_KEY_VAULT_URI environment variable.</param>
        /// <returns>An <see cref="ITokenStore"/> implementation suitable for the detected environment.</returns>
        /// <exception cref="NotSupportedException">Thrown when the detected environment is Unknown or unsupported.</exception>
        /// <exception cref="ArgumentNullException">Thrown when environmentDetector or loggerFactory is null.</exception>
        public static ITokenStore CreateForCurrentEnvironment(
            SpamRemovalAgent.Authentication.Environment.IDeploymentEnvironmentDetector environmentDetector,
            ILoggerFactory loggerFactory,
            string? keyVaultUri = null)
        {
            ArgumentNullException.ThrowIfNull(environmentDetector);
            ArgumentNullException.ThrowIfNull(loggerFactory);

            var environment = environmentDetector.DetectEnvironment();
            return Create(environment, loggerFactory, keyVaultUri);
        }
    }
}
