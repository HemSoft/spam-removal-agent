using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Configuration for OAuth 2.0 authentication with Microsoft identity platform.
/// Loaded from environment variables (preferred) or appsettings.json (fallback).
/// </summary>
public sealed record OAuthConfiguration
{
    /// <summary>
    /// Azure AD tenant ID (GUID format).
    /// Example: "12345678-1234-1234-1234-123456789012"
    /// Can also be "common", "organizations", or "consumers" for multi-tenant scenarios.
    /// </summary>
    [JsonPropertyName("tenant_id")]
    public required string TenantId { get; init; }

    /// <summary>
    /// Azure AD application (client) ID assigned to the app registration.
    /// Example: "87654321-4321-4321-4321-210987654321"
    /// </summary>
    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }

    /// <summary>
    /// OAuth redirect URI for interactive authentication flows.
    /// Required for local Windows deployment, not used for cloud service principal flows.
    /// Example: "http://localhost" or "http://localhost:5000"
    /// </summary>
    [JsonPropertyName("redirect_uri")]
    public string? RedirectUri { get; init; }

    /// <summary>
    /// Client secret for service principal authentication (Azure/GitHub Actions).
    /// MUST be stored in secure storage (Key Vault, GitHub Secrets), never in code.
    /// Not used for interactive authentication (PKCE protects without secret).
    /// </summary>
    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; init; }

    /// <summary>
    /// Space-separated list of Microsoft Graph permission scopes to request.
    /// For interactive: "Mail.ReadWrite offline_access"
    /// For client credentials: "https://graph.microsoft.com/.default"
    /// </summary>
    [JsonPropertyName("scopes")]
    public required string Scopes { get; init; }

    /// <summary>
    /// Authentication mode: Interactive (Windows local) or ServicePrincipal (Azure/GitHub Actions).
    /// Automatically detected by DeploymentEnvironmentDetector if not explicitly set.
    /// </summary>
    [JsonPropertyName("auth_mode")]
    public AuthenticationMode? AuthMode { get; init; }

    /// <summary>
    /// Azure Key Vault URI for cloud deployments (Azure environment only).
    /// Example: "https://mykeyvault.vault.azure.net/"
    /// </summary>
    [JsonPropertyName("key_vault_uri")]
    public string? KeyVaultUri { get; init; }

    /// <summary>
    /// Validates that required fields are populated based on authentication mode.
    /// </summary>
    public ValidationResult Validate(DeploymentEnvironment environment)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(TenantId))
            errors.Add("TenantId is required");

        if (string.IsNullOrWhiteSpace(ClientId))
            errors.Add("ClientId is required");

        if (string.IsNullOrWhiteSpace(Scopes))
            errors.Add("Scopes is required");

        // Environment-specific validation
        if (environment == DeploymentEnvironment.WindowsLocal)
        {
            if (string.IsNullOrWhiteSpace(RedirectUri))
                errors.Add("RedirectUri is required for Windows local deployment");

            if (!Scopes.Contains("offline_access", StringComparison.OrdinalIgnoreCase))
                errors.Add("Scopes must include offline_access for refresh token support");
        }
        else if (environment == DeploymentEnvironment.Azure || environment == DeploymentEnvironment.GitHubActions)
        {
            if (string.IsNullOrWhiteSpace(ClientSecret))
                errors.Add("ClientSecret is required for cloud deployments");

            if (!Scopes.Equals("https://graph.microsoft.com/.default", StringComparison.OrdinalIgnoreCase))
                errors.Add("Scopes must be 'https://graph.microsoft.com/.default' for service principal authentication");
        }

        if (environment == DeploymentEnvironment.Azure && string.IsNullOrWhiteSpace(KeyVaultUri))
        {
            errors.Add("KeyVaultUri is required for Azure deployment");
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }

    /// <summary>
    /// Loads configuration from environment variables with fallback to appsettings.json.
    /// Environment variables take precedence for security (especially for cloud deployments).
    /// </summary>
    public static OAuthConfiguration LoadFromEnvironment(IConfiguration configuration)
    {
        return new OAuthConfiguration
        {
            TenantId = System.Environment.GetEnvironmentVariable("AZURE_TENANT_ID")
                ?? configuration["Authentication:TenantId"]
                ?? throw new InvalidOperationException("AZURE_TENANT_ID not found"),

            ClientId = System.Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
                ?? configuration["Authentication:ClientId"]
                ?? throw new InvalidOperationException("AZURE_CLIENT_ID not found"),

            RedirectUri = System.Environment.GetEnvironmentVariable("OAUTH_REDIRECT_URI")
                ?? configuration["Authentication:RedirectUri"],

            ClientSecret = System.Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")
                ?? configuration["Authentication:ClientSecret"],

            Scopes = System.Environment.GetEnvironmentVariable("OAUTH_SCOPES")
                ?? configuration["Authentication:Scopes"]
                ?? "Mail.ReadWrite offline_access",

            KeyVaultUri = System.Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI")
                ?? configuration["Authentication:KeyVaultUri"]
        };
    }
}
