namespace SpamRemovalAgent.Tests.Utilities;

/// <summary>
/// Builder for mocking environment variables in tests.
/// </summary>
public class TestEnvironmentBuilder
{
    private readonly Dictionary<string, string> _variables = new();

    /// <summary>
    /// Configures environment variables for Windows local environment.
    /// </summary>
    public TestEnvironmentBuilder WithWindowsLocal(string tenantId, string clientId)
    {
        _variables["AZURE_TENANT_ID"] = tenantId;
        _variables["AZURE_CLIENT_ID"] = clientId;
        _variables["OAUTH_REDIRECT_URI"] = "http://localhost";
        _variables["OAUTH_SCOPES"] = "Mail.ReadWrite offline_access";

        // Clear cloud indicators
        ClearCloudVariables();
        return this;
    }

    /// <summary>
    /// Configures environment variables for Azure cloud environment.
    /// </summary>
    public TestEnvironmentBuilder WithAzure(string tenantId, string clientId, string clientSecret, string keyVaultUri)
    {
        _variables["AZURE_TENANT_ID"] = tenantId;
        _variables["AZURE_CLIENT_ID"] = clientId;
        _variables["AZURE_CLIENT_SECRET"] = clientSecret;
        _variables["AZURE_KEY_VAULT_URI"] = keyVaultUri;
        _variables["OAUTH_SCOPES"] = "https://graph.microsoft.com/.default";
        _variables["AZURE_FUNCTIONS_ENVIRONMENT"] = "Production";

        // Clear GitHub Actions indicators
        if (_variables.ContainsKey("GITHUB_ACTIONS"))
            _variables.Remove("GITHUB_ACTIONS");

        return this;
    }

    /// <summary>
    /// Configures environment variables for GitHub Actions environment.
    /// </summary>
    public TestEnvironmentBuilder WithGitHubActions(string tenantId, string clientId, string clientSecret)
    {
        _variables["AZURE_TENANT_ID"] = tenantId;
        _variables["AZURE_CLIENT_ID"] = clientId;
        _variables["AZURE_CLIENT_SECRET"] = clientSecret;
        _variables["OAUTH_SCOPES"] = "https://graph.microsoft.com/.default";
        _variables["GITHUB_ACTIONS"] = "true";
        _variables["GITHUB_REPOSITORY"] = "test-org/test-repo";

        // Clear Azure indicators
        if (_variables.ContainsKey("AZURE_FUNCTIONS_ENVIRONMENT"))
            _variables.Remove("AZURE_FUNCTIONS_ENVIRONMENT");

        return this;
    }

    /// <summary>
    /// Sets a custom environment variable.
    /// </summary>
    public TestEnvironmentBuilder WithVariable(string key, string value)
    {
        _variables[key] = value;
        return this;
    }

    /// <summary>
    /// Clears all cloud environment indicators.
    /// </summary>
    private void ClearCloudVariables()
    {
        var cloudVars = new[] { "AZURE_FUNCTIONS_ENVIRONMENT", "WEBSITE_SITE_NAME", "GITHUB_ACTIONS" };
        foreach (var key in cloudVars)
        {
            if (_variables.ContainsKey(key))
                _variables.Remove(key);
        }
    }

    /// <summary>
    /// Applies the configured environment variables for the test duration.
    /// Returns a disposable that restores original values when disposed.
    /// </summary>
    public IDisposable Apply()
    {
        var original = new Dictionary<string, string?>();

        foreach (var (key, value) in _variables)
        {
            original[key] = System.Environment.GetEnvironmentVariable(key);
            System.Environment.SetEnvironmentVariable(key, value);
        }

        return new EnvironmentRestorer(original);
    }

    private class EnvironmentRestorer : IDisposable
    {
        private readonly Dictionary<string, string?> _original;

        public EnvironmentRestorer(Dictionary<string, string?> original)
        {
            _original = original;
        }

        public void Dispose()
        {
            foreach (var (key, value) in _original)
            {
                System.Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
