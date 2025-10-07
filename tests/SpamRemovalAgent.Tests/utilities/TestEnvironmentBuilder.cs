namespace SpamRemovalAgent.Tests.Utilities;

/// <summary>
/// Builder for mocking environment variables in tests.
/// Ensures complete isolation by explicitly managing both set and cleared variables.
/// </summary>
public class TestEnvironmentBuilder
{
    private readonly Dictionary<string, string?> _variables = new();

    /// <summary>
    /// All authentication-related environment variables that may need to be managed.
    /// </summary>
    private static readonly string[] AllAuthVariables = new[]
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
        "GITHUB_RUN_NUMBER"
    };

    /// <summary>
    /// Configures environment variables for Windows local environment.
    /// </summary>
    public TestEnvironmentBuilder WithWindowsLocal(string tenantId, string clientId)
    {
        // Set required variables
        _variables["AZURE_TENANT_ID"] = tenantId;
        _variables["AZURE_CLIENT_ID"] = clientId;
        _variables["OAUTH_REDIRECT_URI"] = "http://localhost";
        _variables["OAUTH_SCOPES"] = "Mail.ReadWrite offline_access";

        // Explicitly clear cloud indicators
        _variables["AZURE_CLIENT_SECRET"] = null;
        _variables["AZURE_KEY_VAULT_URI"] = null;
        _variables["AZURE_FUNCTIONS_ENVIRONMENT"] = null;
        _variables["WEBSITE_SITE_NAME"] = null;
        _variables["WEBSITE_INSTANCE_ID"] = null;
        _variables["GITHUB_ACTIONS"] = null;
        _variables["GITHUB_REPOSITORY"] = null;

        return this;
    }

    /// <summary>
    /// Configures environment variables for Azure cloud environment.
    /// </summary>
    public TestEnvironmentBuilder WithAzure(string tenantId, string clientId, string clientSecret, string keyVaultUri)
    {
        // Set required variables
        _variables["AZURE_TENANT_ID"] = tenantId;
        _variables["AZURE_CLIENT_ID"] = clientId;
        _variables["AZURE_CLIENT_SECRET"] = clientSecret;
        _variables["AZURE_KEY_VAULT_URI"] = keyVaultUri;
        _variables["OAUTH_SCOPES"] = "https://graph.microsoft.com/.default";
        _variables["AZURE_FUNCTIONS_ENVIRONMENT"] = "Production";

        // Explicitly clear GitHub Actions indicators
        _variables["OAUTH_REDIRECT_URI"] = null;
        _variables["GITHUB_ACTIONS"] = null;
        _variables["GITHUB_REPOSITORY"] = null;

        return this;
    }

    /// <summary>
    /// Configures environment variables for GitHub Actions environment.
    /// </summary>
    public TestEnvironmentBuilder WithGitHubActions(string tenantId, string clientId, string clientSecret)
    {
        // Set required variables
        _variables["AZURE_TENANT_ID"] = tenantId;
        _variables["AZURE_CLIENT_ID"] = clientId;
        _variables["AZURE_CLIENT_SECRET"] = clientSecret;
        _variables["OAUTH_SCOPES"] = "https://graph.microsoft.com/.default";
        _variables["GITHUB_ACTIONS"] = "true";
        _variables["GITHUB_REPOSITORY"] = "test-org/test-repo";

        // Explicitly clear Azure indicators
        _variables["OAUTH_REDIRECT_URI"] = null;
        _variables["AZURE_KEY_VAULT_URI"] = null;
        _variables["AZURE_FUNCTIONS_ENVIRONMENT"] = null;
        _variables["WEBSITE_SITE_NAME"] = null;
        _variables["WEBSITE_INSTANCE_ID"] = null;

        return this;
    }

    /// <summary>
    /// Sets a custom environment variable.
    /// </summary>
    public TestEnvironmentBuilder WithVariable(string key, string? value)
    {
        _variables[key] = value;
        return this;
    }

    /// <summary>
    /// Clears a specific environment variable (sets to null).
    /// </summary>
    public TestEnvironmentBuilder ClearVariable(string key)
    {
        _variables[key] = null;
        return this;
    }

    /// <summary>
    /// Applies the configured environment variables for the test duration.
    /// Returns a disposable that restores original values when disposed.
    /// IMPORTANT: Always use within a 'using' statement to ensure cleanup.
    /// </summary>
    public IDisposable Apply()
    {
        var original = new Dictionary<string, string?>();

        // First, capture original values of ALL auth variables
        foreach (var key in AllAuthVariables)
        {
            original[key] = System.Environment.GetEnvironmentVariable(key);
        }

        // Also capture any custom variables
        foreach (var key in _variables.Keys)
        {
            if (!original.ContainsKey(key))
            {
                original[key] = System.Environment.GetEnvironmentVariable(key);
            }
        }

        // Apply all configured variables (including nulls to clear)
        foreach (var (key, value) in _variables)
        {
            System.Environment.SetEnvironmentVariable(key, value);
        }

        return new EnvironmentRestorer(original);
    }

    private class EnvironmentRestorer : IDisposable
    {
        private readonly Dictionary<string, string?> _original;
        private bool _disposed;

        public EnvironmentRestorer(Dictionary<string, string?> original)
        {
            _original = original;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var (key, value) in _original)
            {
                System.Environment.SetEnvironmentVariable(key, value);
            }

            _disposed = true;
        }
    }
}
