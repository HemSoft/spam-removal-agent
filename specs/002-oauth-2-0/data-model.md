# Data Model: OAuth 2.0 Authentication for Microsoft Graph

**Date**: October 6, 2025
**Branch**: `002-oauth-2-0`
**Related Spec**: [spec.md](./spec.md)
**Related Plan**: [plan.md](./plan.md)
**Related Research**: [research.md](./research.md)

---

## Overview

This document defines the core data models for OAuth 2.0 authentication with Microsoft Graph. All models are designed for serialization (JSON) and immutability where appropriate.

---

## Core Entities

### 1. OAuthToken

Represents a complete set of OAuth 2.0 tokens with expiration metadata.

```csharp
namespace SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Represents an OAuth 2.0 token set including access token, refresh token, and expiration information.
/// </summary>
public sealed record OAuthToken
{
    /// <summary>
    /// The access token used to authenticate Graph API requests.
    /// Format: JWT (3 base64-encoded segments separated by dots).
    /// Lifetime: 60-90 minutes (determined by Microsoft Entra ID).
    /// </summary>
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    /// <summary>
    /// The refresh token used to obtain new access tokens when the current one expires.
    /// Format: Opaque string (not JWT).
    /// Lifetime: 90 days (rolling window) for standard apps, 24 hours for SPA apps.
    /// May be null for client credentials flow (no refresh token issued).
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    /// <summary>
    /// The absolute UTC timestamp when the access token expires.
    /// System should proactively refresh 5 minutes before this time.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public required DateTimeOffset ExpiresAt { get; init; }

    /// <summary>
    /// The space-separated list of Microsoft Graph scopes granted by this token.
    /// Example: "Mail.ReadWrite offline_access User.Read"
    /// </summary>
    [JsonPropertyName("scope")]
    public required string Scope { get; init; }

    /// <summary>
    /// The type of token (always "Bearer" for Microsoft identity platform).
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Optional: The Azure AD correlation ID from the token response.
    /// Used for troubleshooting and audit logging.
    /// </summary>
    [JsonPropertyName("correlation_id")]
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Checks if the access token is expired or expiring within the specified buffer time.
    /// </summary>
    /// <param name="bufferTime">Time buffer before actual expiration (default: 5 minutes)</param>
    /// <returns>True if token needs refresh, false otherwise</returns>
    public bool IsExpiringSoon(TimeSpan? bufferTime = null)
    {
        var buffer = bufferTime ?? TimeSpan.FromMinutes(5);
        return (ExpiresAt - DateTimeOffset.UtcNow) <= buffer;
    }

    /// <summary>
    /// Validates that required fields are populated and values are within expected constraints.
    /// </summary>
    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(AccessToken))
            errors.Add("AccessToken is required");

        if (!AccessToken.Contains('.'))
            errors.Add("AccessToken must be a valid JWT format");

        if (ExpiresAt <= DateTimeOffset.UtcNow)
            errors.Add("Token has already expired");

        if (string.IsNullOrWhiteSpace(Scope))
            errors.Add("Scope is required");

        if (!Scope.Contains("Mail.ReadWrite"))
            errors.Add("Scope must include Mail.ReadWrite permission");

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
```

**Usage Example**:
```csharp
var token = new OAuthToken
{
    AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGc...",
    RefreshToken = "AwABAAAAvPM1KaPlrEqd...",
    ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
    Scope = "Mail.ReadWrite offline_access",
    CorrelationId = "12345678-1234-1234-1234-123456789012"
};

if (token.IsExpiringSoon())
{
    // Proactively refresh token
}
```

---

### 2. OAuthConfiguration

Represents the static configuration required for OAuth 2.0 authentication.

```csharp
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

            if (!Scopes.Contains("offline_access"))
                errors.Add("Scopes must include offline_access for refresh token support");
        }
        else if (environment == DeploymentEnvironment.Azure || environment == DeploymentEnvironment.GitHubActions)
        {
            if (string.IsNullOrWhiteSpace(ClientSecret))
                errors.Add("ClientSecret is required for cloud deployments");

            if (!Scopes.Equals("https://graph.microsoft.com/.default"))
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
            TenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID")
                ?? configuration["Authentication:TenantId"]
                ?? throw new InvalidOperationException("AZURE_TENANT_ID not found"),

            ClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
                ?? configuration["Authentication:ClientId"]
                ?? throw new InvalidOperationException("AZURE_CLIENT_ID not found"),

            RedirectUri = Environment.GetEnvironmentVariable("OAUTH_REDIRECT_URI")
                ?? configuration["Authentication:RedirectUri"],

            ClientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")
                ?? configuration["Authentication:ClientSecret"],

            Scopes = Environment.GetEnvironmentVariable("OAUTH_SCOPES")
                ?? configuration["Authentication:Scopes"]
                ?? "Mail.ReadWrite offline_access",

            KeyVaultUri = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI")
                ?? configuration["Authentication:KeyVaultUri"]
        };
    }
}
```

**Usage Example**:
```csharp
var config = OAuthConfiguration.LoadFromEnvironment(configuration);
var validation = config.Validate(DeploymentEnvironment.Azure);
if (!validation.IsSuccess)
{
    throw new ConfigurationException($"Invalid OAuth configuration: {string.Join(", ", validation.Errors)}");
}
```

---

### 3. AuthenticationResult

Represents the outcome of an authentication attempt.

```csharp
namespace SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Result of an OAuth 2.0 authentication operation.
/// </summary>
public sealed record AuthenticationResult
{
    /// <summary>
    /// Indicates whether authentication was successful.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// The obtained access token (only populated if IsSuccess = true).
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// The complete OAuth token set (only populated if IsSuccess = true).
    /// </summary>
    public OAuthToken? Token { get; init; }

    /// <summary>
    /// Error code if authentication failed (e.g., "invalid_grant", "consent_required").
    /// Maps to standard OAuth 2.0 error codes or custom application error codes.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Human-readable error message if authentication failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Azure AD correlation ID for troubleshooting.
    /// Present in both success and failure scenarios.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Time taken to complete the authentication operation.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Indicates whether the failure is permanent (should not retry).
    /// Examples: consent_required, invalid_client, access_denied.
    /// </summary>
    public bool IsPermanentFailure { get; init; }

    /// <summary>
    /// Creates a successful authentication result.
    /// </summary>
    public static AuthenticationResult Success(OAuthToken token, TimeSpan duration, string? correlationId = null)
    {
        return new AuthenticationResult
        {
            IsSuccess = true,
            AccessToken = token.AccessToken,
            Token = token,
            Duration = duration,
            CorrelationId = correlationId ?? token.CorrelationId
        };
    }

    /// <summary>
    /// Creates a failed authentication result with retry eligibility.
    /// </summary>
    public static AuthenticationResult Failure(
        string errorCode,
        string errorMessage,
        TimeSpan duration,
        bool isPermanent = false,
        string? correlationId = null)
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Duration = duration,
            IsPermanentFailure = isPermanent,
            CorrelationId = correlationId
        };
    }
}
```

**Usage Example**:
```csharp
try
{
    var token = await authenticator.AuthenticateAsync(cancellationToken);
    return AuthenticationResult.Success(token, stopwatch.Elapsed);
}
catch (PermanentAuthFailureException ex)
{
    return AuthenticationResult.Failure(ex.ErrorCode, ex.Message, stopwatch.Elapsed, isPermanent: true);
}
```

---

## Supporting Enumerations

### 4. DeploymentEnvironment

```csharp
namespace SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Represents the deployment environment for the application.
/// Determines which token storage mechanism and authentication flow to use.
/// </summary>
public enum DeploymentEnvironment
{
    /// <summary>
    /// Unknown or unsupported environment.
    /// System should fail fast if this is detected.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Windows 10+ local machine deployment.
    /// Uses interactive OAuth flow with browser and Windows Credential Manager for token storage.
    /// </summary>
    WindowsLocal = 1,

    /// <summary>
    /// Azure cloud deployment (App Service, Container Instances, VMs).
    /// Uses service principal authentication and Azure Key Vault for token storage.
    /// </summary>
    Azure = 2,

    /// <summary>
    /// GitHub Actions workflow execution.
    /// Uses service principal authentication and GitHub Secrets for token storage.
    /// </summary>
    GitHubActions = 3
}
```

---

### 5. AuthenticationMode

```csharp
namespace SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Represents the OAuth 2.0 authentication flow mode.
/// </summary>
public enum AuthenticationMode
{
    /// <summary>
    /// Interactive authorization code flow with PKCE (for local development).
    /// Requires user interaction via browser consent.
    /// Provides refresh token for long-lived access.
    /// </summary>
    Interactive = 1,

    /// <summary>
    /// Service principal authentication with client credentials (for cloud deployments).
    /// Non-interactive, uses client ID and client secret.
    /// No refresh token (must re-authenticate when access token expires).
    /// </summary>
    ServicePrincipal = 2
}
```

---

## Validation Helper

### 6. ValidationResult

```csharp
namespace SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public sealed record ValidationResult
{
    /// <summary>
    /// Indicates whether validation passed.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// List of validation error messages (empty if IsSuccess = true).
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new ValidationResult { IsSuccess = true };

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    public static ValidationResult Failure(IEnumerable<string> errors) => new ValidationResult
    {
        IsSuccess = false,
        Errors = errors.ToList()
    };
}
```

---

## Entity Relationships

```
OAuthConfiguration
  ↓ (used by)
OAuthAuthenticator
  ↓ (produces)
OAuthToken
  ↓ (stored by)
ITokenStore (WindowsCredentialStore / AzureKeyVaultTokenStore / GitHubSecretsTokenStore)
  ↓ (retrieved by)
TokenRefreshMiddleware
  ↓ (returns)
AuthenticationResult
```

---

## State Transitions: Token Lifecycle

```
[No Token]
  → Authenticate()
  → [Valid Token]
  → (Time passes - 55min)
  → [Token Expiring Soon]
  → RefreshToken()
  → [Valid Token]
  → ...repeat...

[Valid Token]
  → (Refresh token expires/revoked)
  → [Permanent Failure]
  → ClearToken()
  → [No Token]
  → (Requires re-authentication)
```

---

## JSON Serialization Examples

### OAuthToken (stored in Windows Credential Manager / Key Vault)
```json
{
  "access_token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik5HVEZ2...",
  "refresh_token": "AwABAAAAvPM1KaPlrEqdFSBzjqfTGAMxZGUTdM0t4B4...",
  "expires_at": "2025-10-06T14:30:00Z",
  "scope": "Mail.ReadWrite offline_access",
  "token_type": "Bearer",
  "correlation_id": "12345678-1234-1234-1234-123456789012"
}
```

### OAuthConfiguration (appsettings.json fallback)
```json
{
  "Authentication": {
    "TenantId": "12345678-1234-1234-1234-123456789012",
    "ClientId": "87654321-4321-4321-4321-210987654321",
    "RedirectUri": "http://localhost",
    "Scopes": "Mail.ReadWrite offline_access",
    "KeyVaultUri": "https://mykeyvault.vault.azure.net/"
  }
}
```

---

## Validation Rules Summary

| **Entity** | **Required Fields** | **Conditional Requirements** |
|------------|---------------------|------------------------------|
| **OAuthToken** | AccessToken, ExpiresAt, Scope | RefreshToken (if not client credentials flow) |
| **OAuthConfiguration** | TenantId, ClientId, Scopes | RedirectUri (Windows local), ClientSecret (Azure/GitHub), KeyVaultUri (Azure) |
| **AuthenticationResult** | IsSuccess, Duration | AccessToken/Token (if success), ErrorCode/ErrorMessage (if failure) |

---

## Security Considerations

1. **Token Storage**: Never store tokens in plaintext. Always use secure storage mechanisms:
   - Windows: ProtectedData API with DataProtectionScope.CurrentUser
   - Azure: Key Vault with managed identity
   - GitHub Actions: GitHub Secrets (read-only at runtime)

2. **Logging**: Never log token values. Redact any value matching JWT format or long base64 strings.

3. **Serialization**: Use `[JsonIgnore]` or secure serializers for in-memory token objects.

4. **Validation**: Always validate tokens before use and configuration before authentication attempts.

---

**Document Status**: ✅ Complete | **Last Updated**: October 6, 2025
