namespace SpamRemovalAgent.Tests.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using SpamRemovalAgent.Observability;
    using Xunit;

    /// <summary>
    /// Base class for authentication integration tests that provides:
    /// - Environment variable cleanup before/after each test
    /// - Common test infrastructure (logger, telemetry mocks)
    /// - Consistent configuration building
    /// </summary>
    public abstract class AuthenticationTestBase : IAsyncLifetime
    {
        /// <summary>
        /// All authentication-related environment variables that should be cleared between tests.
        /// </summary>
        private static readonly string[] AuthEnvironmentVariables = new[]
        {
            "AZURE_TENANT_ID",
            "AZURE_CLIENT_ID",
            "AZURE_CLIENT_SECRET",
            "AZURE_KEY_VAULT_URI",
            "OAUTH_REDIRECT_URI",
            "OAUTH_SCOPES",
            "AZURE_FUNCTIONS_ENVIRONMENT",
            "WEBSITE_SITE_NAME",
            "WEBSITE_INSTANCE_ID",
            "GITHUB_ACTIONS",
            "GITHUB_REPOSITORY",
            "GITHUB_RUN_ID",
            "GITHUB_RUN_NUMBER",
            "RUN_INTEGRATION_TESTS"
        };

        private readonly Dictionary<string, string?> _originalEnvironmentVariables = new();

        protected ILoggerFactory LoggerFactory { get; private set; } = null!;
        protected Mock<IAuthenticationTelemetry> MockTelemetry { get; private set; } = null!;

        /// <summary>
        /// Called before each test runs. Captures current environment state for restoration after test.
        /// </summary>
        public virtual ValueTask InitializeAsync()
        {
            // Capture original environment variable values for restoration after test
            foreach (var key in AuthEnvironmentVariables)
            {
                _originalEnvironmentVariables[key] = Environment.GetEnvironmentVariable(key);
            }

            // Initialize common test infrastructure
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            MockTelemetry = new Mock<IAuthenticationTelemetry>();

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Called after each test completes. Restores original environment state.
        /// </summary>
        public virtual ValueTask DisposeAsync()
        {
            // Restore original environment variable values
            foreach (var (key, value) in _originalEnvironmentVariables)
            {
                Environment.SetEnvironmentVariable(key, value);
            }

            _originalEnvironmentVariables.Clear();

            // Dispose test infrastructure
            LoggerFactory?.Dispose();

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Clears all authentication-related environment variables.
        /// </summary>
        protected static void ClearAllAuthEnvironmentVariables()
        {
            foreach (var key in AuthEnvironmentVariables)
            {
                Environment.SetEnvironmentVariable(key, null);
            }
        }

        /// <summary>
        /// Builds a configuration for Windows local environment testing.
        /// </summary>
        protected static IConfiguration BuildWindowsLocalConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["Authentication:TenantId"] = "",
                ["Authentication:ClientId"] = "",
                ["Authentication:RedirectUri"] = "http://localhost",
                ["Authentication:Scopes"] = "Mail.ReadWrite offline_access",
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        /// <summary>
        /// Builds a configuration for Azure cloud environment testing.
        /// </summary>
        protected static IConfiguration BuildAzureConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["Authentication:TenantId"] = "",
                ["Authentication:ClientId"] = "",
                ["Authentication:ClientSecret"] = "",
                ["Authentication:Scopes"] = "https://graph.microsoft.com/.default",
                ["Authentication:KeyVaultUri"] = "",
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        /// <summary>
        /// Builds a configuration for GitHub Actions environment testing.
        /// </summary>
        protected static IConfiguration BuildGitHubActionsConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["Authentication:TenantId"] = "",
                ["Authentication:ClientId"] = "",
                ["Authentication:ClientSecret"] = "",
                ["Authentication:Scopes"] = "https://graph.microsoft.com/.default",
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }
    }
}
