# Research: OAuth 2.0 Authentication for Microsoft Graph

**Date**: October 6, 2025
**Branch**: `002-oauth-2-0`
**Related Spec**: [spec.md](./spec.md)
**Related Plan**: [plan.md](./plan.md)

---

## Research Questions Resolved

This document consolidates research findings for all technical unknowns identified in the feature specification and technical context.

---

## 1. OAuth 2.0 Authorization Code Flow with Microsoft Identity Platform

### Decision
Use **OAuth 2.0 authorization code flow with PKCE (Proof Key for Code Exchange)** for interactive authentication and **client credentials flow (service principal)** for non-interactive cloud deployments.

### Rationale
- **Authorization Code + PKCE**: Recommended by Microsoft for all application types (including native/desktop apps) as the most secure OAuth flow. PKCE mitigates authorization code interception attacks without requiring client secrets on the client side.
- **Client Credentials**: Required for automated cloud environments (Azure, GitHub Actions) where interactive user consent is not possible. Uses service principal with client secret or certificate.
- **Hybrid Approach**: Allows same codebase to support both local development (interactive) and production deployment (automated).

### Implementation Details

#### Authorization Code Flow (Interactive - Windows Local)
```
1. Application redirects user to /authorize endpoint
   → Parameters: client_id, scope (Mail.ReadWrite + offline_access), redirect_uri, code_challenge (PKCE), response_type=code
2. User authenticates and grants consent
3. Microsoft Entra ID returns authorization code to redirect_uri
4. Application exchanges code for tokens at /token endpoint
   → Parameters: code, code_verifier (PKCE), client_id, grant_type=authorization_code
5. Response: access_token, refresh_token, expires_in
```

#### Client Credentials Flow (Non-Interactive - Azure/GitHub Actions)
```
1. Application authenticates with service principal credentials
   → Parameters: client_id, client_secret, scope (https://graph.microsoft.com/.default), grant_type=client_credentials
2. Microsoft Entra ID validates credentials
3. Response: access_token, expires_in (no refresh_token - must re-authenticate)
```

### Key API Endpoints
- **Authorization**: `https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize`
- **Token**: `https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token`
- **Microsoft Graph**: `https://graph.microsoft.com/v1.0/`

### Alternatives Considered
- **Implicit Flow**: Deprecated by Microsoft, less secure (tokens in URL fragments)
- **Device Code Flow**: Better for limited-input devices, but unnecessary complexity for our scenarios
- **On-Behalf-Of Flow**: For delegation scenarios (not applicable here)
- **Managed Identity**: Considered for Azure, but service principal provides more flexibility for multi-environment consistency

### References
- [Microsoft Identity Platform OAuth 2.0 Authorization Code Flow](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow)
- [Get access on behalf of a user - Microsoft Graph](https://learn.microsoft.com/en-us/graph/auth-v2-user)
- [PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)

---

## 2. Azure Identity SDK for .NET - Best Practices

### Decision
Use **Azure.Identity** SDK (`DefaultAzureCredential` and environment-specific credential types) as the primary authentication library, with **Microsoft.Identity.Client (MSAL)** for low-level OAuth operations when needed.

### Rationale
- **Azure.Identity**: Provides unified credential chain pattern (`DefaultAzureCredential`) that automatically tries multiple authentication methods in sequence. Simplifies code for multi-environment support.
- **MSAL.NET**: Lower-level library for fine-grained control over OAuth flows. Used when `DefaultAzureCredential` is insufficient (e.g., interactive browser authentication with PKCE).
- **Token Caching**: Both libraries support in-memory and persistent token caching, reducing authentication overhead.
- **CAE Support**: Continuous Access Evaluation (CAE) supported via `IsCaeEnabled` property for enhanced security.

### Implementation Patterns

#### DefaultAzureCredential for Simple Scenarios
```csharp
using Azure.Identity;
using Microsoft.Graph;

// Automatically tries: EnvironmentCredential → ManagedIdentityCredential → AzureCliCredential → etc.
var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"),
    TenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID")
});

var graphClient = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });
```

#### MSAL for Interactive Flow with PKCE
```csharp
using Microsoft.Identity.Client;

var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
    .WithRedirectUri("http://localhost") // or custom redirect
    .Build();

var result = await app.AcquireTokenInteractive(new[] { "Mail.ReadWrite", "offline_access" })
    .WithPkceAsync(); // PKCE enabled by default in MSAL 4.x+

string accessToken = result.AccessToken;
string refreshToken = result.Account.HomeAccountId; // Use for future silent token acquisition
```

#### Service Principal Authentication
```csharp
using Azure.Identity;

// Option 1: Client Secret
var credential = new ClientSecretCredential(
    tenantId: Environment.GetEnvironmentVariable("AZURE_TENANT_ID"),
    clientId: Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"),
    clientSecret: Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")
);

// Option 2: Client Certificate (more secure)
var credential = new ClientCertificateCredential(
    tenantId: tenantId,
    clientId: clientId,
    certificatePath: "/path/to/cert.pfx",
    certificatePassword: certPassword
);
```

### Token Caching Strategy
- **In-Memory Cache**: Default for all credential types (stores tokens in process memory)
- **Persistent Cache**: Optional, uses DPAPI on Windows for encrypted storage
- **Cache Expiration**: Proactively refresh 5 minutes before expiration
- **Thread Safety**: All Azure.Identity credential types are thread-safe

### Alternatives Considered
- **Microsoft.Graph SDK built-in auth**: Limited flexibility for multi-environment scenarios
- **Manual HTTP requests**: Too low-level, error-prone, loses built-in retry/throttling logic
- **Azure.Core.TokenCredential interface only**: Requires more manual token management

### References
- [Azure Identity client library for .NET](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme)
- [DefaultAzureCredential Overview](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential)
- [MSAL.NET Documentation](https://learn.microsoft.com/en-us/entra/identity-platform/msal-overview)

---

## 3. Multi-Environment Token Storage

### Decision
Use **environment-specific secure storage** with abstraction layer:
- **Windows Local**: Windows Credential Manager via `System.Security.Cryptography.ProtectedData` (DPAPI)
- **Azure Cloud**: Azure Key Vault with managed identity authentication
- **GitHub Actions**: GitHub Secrets accessed via environment variables at workflow runtime

### Rationale
- **Security**: All three mechanisms meet enterprise security standards for credential storage
- **Platform Compatibility**: Each storage type is native to its environment
- **Isolation**: Tokens are environment-specific and not portable (by design)
- **No Shared State**: Each deployment instance maintains independent credentials

### Implementation Details

#### Windows Credential Manager (Local Development)
```csharp
using System.Security.Cryptography;
using System.Text;

public class WindowsCredentialStore : ITokenStore
{
    private const string TARGET_NAME = "SpamRemovalAgent.OAuth";

    public async Task<OAuthToken> RetrieveTokenAsync(CancellationToken cancellationToken)
    {
        // Use ProtectedData.Unprotect with DataProtectionScope.CurrentUser
        // Tokens encrypted with user's Windows credentials
        byte[] encryptedData = ReadFromWindowsVault(TARGET_NAME);
        byte[] decryptedData = ProtectedData.Unprotect(encryptedData,
            entropy: null,
            DataProtectionScope.CurrentUser);

        string json = Encoding.UTF8.GetString(decryptedData);
        return JsonSerializer.Deserialize<OAuthToken>(json);
    }

    public async Task StoreTokenAsync(OAuthToken token, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(token);
        byte[] plainData = Encoding.UTF8.GetBytes(json);
        byte[] encryptedData = ProtectedData.Protect(plainData,
            entropy: null,
            DataProtectionScope.CurrentUser);

        WriteToWindowsVault(TARGET_NAME, encryptedData);
    }

    private byte[] ReadFromWindowsVault(string targetName)
    {
        // P/Invoke to CredRead from Advapi32.dll
        // Or use third-party library like CredentialManagement
    }
}
```

**Pros**:
- Native Windows integration
- User-scoped encryption (no plaintext secrets)
- Persists across application restarts
- No external dependencies

**Cons**:
- Windows-only (not portable)
- Requires P/Invoke or third-party library
- Limited cloud compatibility

#### Azure Key Vault (Azure Deployment)
```csharp
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

public class AzureKeyVaultTokenStore : ITokenStore
{
    private readonly SecretClient _client;

    public AzureKeyVaultTokenStore(string keyVaultUri)
    {
        // Use DefaultAzureCredential to authenticate with managed identity
        var credential = new DefaultAzureCredential();
        _client = new SecretClient(new Uri(keyVaultUri), credential);
    }

    public async Task<OAuthToken> RetrieveTokenAsync(CancellationToken cancellationToken)
    {
        KeyVaultSecret secret = await _client.GetSecretAsync("oauth-token", cancellationToken: cancellationToken);
        return JsonSerializer.Deserialize<OAuthToken>(secret.Value);
    }

    public async Task StoreTokenAsync(OAuthToken token, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(token);
        await _client.SetSecretAsync("oauth-token", json, cancellationToken);
    }
}
```

**Pros**:
- Enterprise-grade security (HSM-backed)
- Managed identity authentication (no secrets in code)
- Centralized secret management
- Audit logging built-in
- Automatic secret rotation support

**Cons**:
- Azure-specific (not portable to other clouds)
- Requires Azure subscription and Key Vault setup
- Network latency for token retrieval

#### GitHub Secrets (GitHub Actions Workflows)
```csharp
public class GitHubSecretsTokenStore : ITokenStore
{
    // GitHub Secrets are injected as environment variables at workflow runtime
    public async Task<OAuthToken> RetrieveTokenAsync(CancellationToken cancellationToken)
    {
        string accessToken = Environment.GetEnvironmentVariable("OAUTH_ACCESS_TOKEN")
            ?? throw new TokenStoreUnavailableException("OAUTH_ACCESS_TOKEN not found in environment");
        string refreshToken = Environment.GetEnvironmentVariable("OAUTH_REFRESH_TOKEN");
        string expiryString = Environment.GetEnvironmentVariable("OAUTH_EXPIRY");

        return new OAuthToken
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTimeOffset.Parse(expiryString)
        };
    }

    public async Task StoreTokenAsync(OAuthToken token, CancellationToken cancellationToken)
    {
        // GitHub Secrets are read-only at runtime
        // To update: Use GitHub CLI or API outside the workflow
        throw new NotSupportedException("GitHub Secrets cannot be updated at runtime. Use GitHub CLI: gh secret set OAUTH_ACCESS_TOKEN");
    }
}
```

**Workflow Example**:
```yaml
name: Run Spam Removal Agent
on:
  schedule:
    - cron: '0 */4 * * *'  # Every 4 hours

jobs:
  run-agent:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Run agent
        env:
          AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
          AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
          AZURE_CLIENT_SECRET: ${{ secrets.AZURE_CLIENT_SECRET }}
          OAUTH_ACCESS_TOKEN: ${{ secrets.OAUTH_ACCESS_TOKEN }}
          OAUTH_REFRESH_TOKEN: ${{ secrets.OAUTH_REFRESH_TOKEN }}
          OAUTH_EXPIRY: ${{ secrets.OAUTH_EXPIRY }}
        run: dotnet run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj
```

**Pros**:
- Native GitHub Actions integration
- Encrypted at rest
- Scoped to repository/organization
- Easy rotation via GitHub UI or CLI

**Cons**:
- GitHub-specific (not portable)
- Manual secret updates required (no automatic token refresh persistence)
- Limited to 48KB per secret

### Token Store Factory Pattern
```csharp
public class TokenStoreFactory
{
    public static ITokenStore Create(DeploymentEnvironment environment)
    {
        return environment switch
        {
            DeploymentEnvironment.WindowsLocal => new WindowsCredentialStore(),
            DeploymentEnvironment.Azure => new AzureKeyVaultTokenStore(
                Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI")
                ?? throw new InvalidOperationException("AZURE_KEY_VAULT_URI required for Azure deployment")),
            DeploymentEnvironment.GitHubActions => new GitHubSecretsTokenStore(),
            _ => throw new NotSupportedException($"Unsupported environment: {environment}")
        };
    }
}
```

### Alternatives Considered
- **File-based storage with encryption**: More portable but less secure (key management complexity)
- **Database storage**: Overkill for single-instance credential storage
- **HashiCorp Vault**: Third-party dependency, adds operational complexity
- **AWS Secrets Manager**: Not aligned with Azure-first strategy

### References
- [System.Security.Cryptography.ProtectedData - DPAPI](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata)
- [Azure Key Vault Overview](https://learn.microsoft.com/en-us/azure/key-vault/general/overview)
- [Using secrets in GitHub Actions](https://docs.github.com/en/actions/security-guides/using-secrets-in-github-actions)

---

## 4. Token Refresh and Lifetime Management

### Decision
Implement **proactive token refresh** 5 minutes before expiration with automatic fallback to full re-authentication on refresh failure.

### Rationale
- **Proactive Refresh**: Prevents service interruption by refreshing before token expires
- **5-Minute Buffer**: Accounts for clock skew and network latency
- **Automatic Fallback**: Handles refresh token expiration/revocation gracefully
- **Telemetry**: Track refresh frequency and failures for operational monitoring

### Implementation Pattern
```csharp
public class TokenRefreshMiddleware
{
    private readonly ITokenStore _tokenStore;
    private readonly IOAuthAuthenticator _authenticator;
    private readonly ILogger<TokenRefreshMiddleware> _logger;
    private readonly TimeSpan _refreshBuffer = TimeSpan.FromMinutes(5);

    public async Task<string> GetValidAccessTokenAsync(CancellationToken cancellationToken)
    {
        var token = await _tokenStore.RetrieveTokenAsync(cancellationToken);

        if (token == null || string.IsNullOrEmpty(token.AccessToken))
        {
            _logger.LogInformation("No token found, initiating full authentication");
            return await _authenticator.AuthenticateAsync(cancellationToken);
        }

        if (IsTokenExpiringSoon(token))
        {
            _logger.LogInformation("Token expiring in {TimeRemaining}, refreshing proactively",
                token.ExpiresAt - DateTimeOffset.UtcNow);

            try
            {
                return await RefreshTokenAsync(token, cancellationToken);
            }
            catch (PermanentAuthFailureException ex)
            {
                _logger.LogWarning(ex, "Refresh token invalid, clearing credentials and requiring re-authentication");
                await _tokenStore.ClearTokenAsync(cancellationToken);
                throw new AuthenticationRequiredException(
                    "Refresh token expired. Please re-authenticate.", ex);
            }
        }

        return token.AccessToken;
    }

    private bool IsTokenExpiringSoon(OAuthToken token)
    {
        return (token.ExpiresAt - DateTimeOffset.UtcNow) <= _refreshBuffer;
    }

    private async Task<string> RefreshTokenAsync(OAuthToken token, CancellationToken cancellationToken)
    {
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token");
        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", token.RefreshToken),
            new KeyValuePair<string, string>("scope", "Mail.ReadWrite offline_access")
        });

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new PermanentAuthFailureException("Refresh token rejected by Microsoft Entra ID");
        }

        var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(
            await response.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);

        var newToken = new OAuthToken
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken ?? token.RefreshToken, // Some responses don't include new refresh token
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
        };

        await _tokenStore.StoreTokenAsync(newToken, cancellationToken);
        return newToken.AccessToken;
    }
}
```

### Token Lifetimes (Microsoft Identity Platform Defaults)
- **Access Token**: 60-90 minutes (exact lifetime varies, cannot be configured)
- **Refresh Token**:
  - Standard: 90 days of inactivity (rolling window)
  - Single-page apps (SPA redirect type): 24 hours (non-rolling)
  - Client credentials: N/A (no refresh token, re-authenticate instead)
- **Authorization Code**: 10 minutes (short-lived by design)

### Refresh Token Expiration Scenarios
1. **User revokes consent**: Refresh token immediately invalid
2. **Password change**: Refresh token may be invalidated (depends on org policy)
3. **Token lifetime exceeded**: 90 days of no use (standard apps)
4. **Manual revocation**: Admin or user explicitly revokes app access

### Alternatives Considered
- **Reactive Refresh (on 401)**: Causes service interruption, poor user experience
- **Always Refresh**: Excessive API calls, increases throttling risk
- **Longer Buffer (10+ minutes)**: Wastes refresh operations, reduces token lifetime utility
- **Shorter Buffer (<2 minutes)**: Increases risk of race conditions with clock skew

### References
- [OAuth 2.0 Refresh Token Best Practices](https://learn.microsoft.com/en-us/entra/identity-platform/refresh-tokens)
- [Microsoft Identity Platform Token Lifetimes](https://learn.microsoft.com/en-us/entra/identity-platform/configurable-token-lifetimes)

---

## 5. Error Handling and Retry Strategies

### Decision
Implement **exponential backoff with jitter** for transient errors and **fail-fast** for permanent failures with environment-specific recovery instructions.

### Rationale
- **Exponential Backoff**: Industry-standard pattern for handling transient network failures
- **Jitter**: Prevents thundering herd problem when multiple clients retry simultaneously
- **Circuit Breaker**: Stops retrying after repeated failures to prevent cascading failures
- **HTTP 429 Handling**: Respects Microsoft Graph throttling with `Retry-After` header

### Retry Policy Implementation
```csharp
public class RetryPolicyMiddleware
{
    private static readonly int[] BackoffDelaysMs = { 2000, 4000, 8000, 16000, 32000 };
    private const int MaxRetries = 5;
    private readonly Random _jitterRandom = new Random();

    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        Exception lastException = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (HttpRequestException ex) when (IsTransientError(ex))
            {
                lastException = ex;

                if (attempt == MaxRetries)
                {
                    throw new TransientAuthFailureException(
                        $"Authentication failed after {MaxRetries} retries", ex);
                }

                int delayMs = BackoffDelaysMs[attempt];
                int jitterMs = _jitterRandom.Next(0, 1000); // 0-1s jitter
                await Task.Delay(delayMs + jitterMs, cancellationToken);
            }
            catch (PermanentAuthFailureException)
            {
                // Don't retry permanent failures
                throw;
            }
        }

        throw lastException;
    }

    private bool IsTransientError(HttpRequestException ex)
    {
        if (ex.StatusCode.HasValue)
        {
            return ex.StatusCode.Value == HttpStatusCode.TooManyRequests // 429
                || ex.StatusCode.Value == HttpStatusCode.ServiceUnavailable // 503
                || ex.StatusCode.Value == HttpStatusCode.GatewayTimeout; // 504
        }

        return ex.InnerException is SocketException
            || ex.InnerException is TimeoutException;
    }
}
```

### HTTP 429 Throttling Handler
```csharp
public class ThrottlingMiddleware
{
    public async Task<HttpResponseMessage> SendWithThrottlingAsync(
        HttpClient client,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            if (response.Headers.TryGetValues("Retry-After", out var values))
            {
                if (int.TryParse(values.First(), out int retryAfterSeconds))
                {
                    _logger.LogWarning("Microsoft Graph throttled request. Retrying after {RetryAfterSeconds} seconds",
                        retryAfterSeconds);
                    await Task.Delay(TimeSpan.FromSeconds(retryAfterSeconds), cancellationToken);
                    return await client.SendAsync(request, cancellationToken);
                }
            }
        }

        return response;
    }
}
```

### Permanent Failure Handling
```csharp
public class PermanentAuthFailureException : Exception
{
    public string ErrorCode { get; }
    public string CorrelationId { get; }
    public DeploymentEnvironment Environment { get; }

    public PermanentAuthFailureException(string message, string errorCode, string correlationId,
        DeploymentEnvironment environment) : base(message)
    {
        ErrorCode = errorCode;
        CorrelationId = correlationId;
        Environment = environment;
    }

    public override string Message => $"{base.Message}\n\n{GetRecoveryInstructions()}";

    private string GetRecoveryInstructions()
    {
        return Environment switch
        {
            DeploymentEnvironment.WindowsLocal =>
                "RESOLUTION: Re-run interactive authentication to re-grant consent:\n" +
                "1. Stop the application\n" +
                "2. Delete stored credentials from Windows Credential Manager\n" +
                "3. Restart the application and complete the login flow",

            DeploymentEnvironment.Azure =>
                "RESOLUTION: Update service principal credentials in Azure Key Vault:\n" +
                "1. Verify service principal is active: az ad sp show --id <client-id>\n" +
                "2. Generate new client secret: az ad sp credential reset --id <client-id>\n" +
                "3. Update Key Vault secret: az keyvault secret set --vault-name <vault> --name azure-client-secret --value <new-secret>\n" +
                "4. Restart the application",

            DeploymentEnvironment.GitHubActions =>
                "RESOLUTION: Update service principal credentials in GitHub Secrets:\n" +
                "1. Generate new client secret: az ad sp credential reset --id <client-id>\n" +
                "2. Update GitHub Secret: gh secret set AZURE_CLIENT_SECRET --body <new-secret>\n" +
                "3. Re-run the workflow",

            _ => "RESOLUTION: Contact system administrator"
        };
    }
}
```

### Alternatives Considered
- **Fixed Retry Delays**: Less resilient to sustained outages
- **Infinite Retries**: Can cause application hangs
- **No Jitter**: Increases risk of synchronized retry storms
- **Retry All Errors**: Wastes resources on unrecoverable errors (4xx client errors)

### References
- [Exponential Backoff and Jitter](https://aws.amazon.com/blogs/architecture/exponential-backoff-and-jitter/)
- [Microsoft Graph Throttling Guidance](https://learn.microsoft.com/en-us/graph/throttling)
- [Polly - .NET Resilience Library](https://github.com/App-vNext/Polly)

---

## 6. Azure AD App Registration Requirements

### Decision
Create **two Azure AD app registrations** (one for local development, one for cloud deployment) with specific configurations for each environment.

### Rationale
- **Separation of Concerns**: Local dev uses interactive consent, cloud uses automated service principal
- **Security Boundaries**: Different redirect URIs and permission scopes prevent cross-contamination
- **Audit Trail**: Separate registrations enable clear tracking of which environment accessed resources

### Local Development App Registration
```
Application (client) ID: <generated-guid>
Directory (tenant) ID: <your-tenant-id>
Supported account types: Single tenant (your organization only)

Authentication:
  - Platform: Mobile and desktop applications
  - Redirect URI: http://localhost (or custom port, e.g., http://localhost:5000)
  - Advanced settings:
    - Allow public client flows: Yes (enables PKCE)
    - Supported account types: Single tenant only

API Permissions:
  - Microsoft Graph:
    - Mail.ReadWrite (Delegated) - Required
    - offline_access (Delegated) - Required for refresh token
    - User.Read (Delegated) - Optional, for profile info

Certificates & Secrets:
  - Not required for interactive flow (PKCE protects without secret)
```

### Cloud Deployment App Registration (Service Principal)
```
Application (client) ID: <generated-guid>
Directory (tenant) ID: <your-tenant-id>
Supported account types: Single tenant (your organization only)

Authentication:
  - Platform: Not applicable (service principal)
  - No redirect URIs

API Permissions:
  - Microsoft Graph:
    - Mail.ReadWrite (Application) - Note: Application permission, not Delegated
    - Grant admin consent required

Certificates & Secrets:
  - Client Secret: <generated-secret> (store in Key Vault/GitHub Secrets)
  - Expiration: Recommend 6-12 months with rotation plan
  - Alternative: Client certificate (more secure than secret)
```

### Key Differences: Delegated vs Application Permissions
- **Delegated (Interactive)**: App acts on behalf of a signed-in user. User's permissions govern access.
- **Application (Service Principal)**: App acts as itself without user context. Requires admin consent.

### Admin Consent Process
For cloud deployments, a tenant administrator must grant consent:
```bash
# Via Azure CLI
az ad app permission admin-consent --id <app-id>

# Via Admin Consent URL
https://login.microsoftonline.com/{tenant}/adminconsent?client_id={client_id}
```

### Configuration Storage
```jsonc
// appsettings.json (fallback only, prefer environment variables)
{
  "Authentication": {
    "Local": {
      "TenantId": "common", // or specific tenant ID
      "ClientId": "<local-app-client-id>",
      "RedirectUri": "http://localhost",
      "Scopes": ["Mail.ReadWrite", "offline_access"]
    },
    "Cloud": {
      "TenantId": "<tenant-id>",
      "ClientId": "<cloud-app-client-id>",
      "Scopes": ["https://graph.microsoft.com/.default"] // Required for client credentials
    }
  }
}
```

### Alternatives Considered
- **Single App Registration**: More complex configuration, harder to audit
- **Multi-Tenant App**: Unnecessary complexity for single-organization deployment
- **Client Certificate Instead of Secret**: More secure but requires certificate management infrastructure

### References
- [Register an application with Microsoft identity platform](https://learn.microsoft.com/en-us/graph/auth-register-app-v2)
- [Microsoft Graph permissions reference](https://learn.microsoft.com/en-us/graph/permissions-reference)

---

## 7. Observability and Telemetry

### Decision
Use **Application Insights SDK** for production telemetry with structured logging via **ILogger<T>** and custom events for authentication lifecycle tracking.

### Rationale
- **Application Insights**: Native Azure integration, powerful query language (KQL), automatic correlation
- **ILogger<T>**: .NET standard logging abstraction, easily testable, supports multiple sinks
- **Custom Events**: Track authentication-specific metrics (success rate, latency, token refresh frequency)
- **Sensitive Data Redaction**: Never log token values, only metadata

### Application Insights Integration
```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

public class AuthenticationTelemetry
{
    private readonly TelemetryClient _telemetry;
    private readonly ILogger<AuthenticationTelemetry> _logger;

    public void TrackAuthenticationStart(string authFlowType, DeploymentEnvironment environment)
    {
        var properties = new Dictionary<string, string>
        {
            ["AuthFlowType"] = authFlowType,
            ["Environment"] = environment.ToString()
        };

        _telemetry.TrackEvent("AuthenticationStarted", properties);
        _logger.LogInformation("Authentication started: Flow={AuthFlowType}, Environment={Environment}",
            authFlowType, environment);
    }

    public void TrackAuthenticationSuccess(string authFlowType, TimeSpan duration, string correlationId)
    {
        var properties = new Dictionary<string, string>
        {
            ["AuthFlowType"] = authFlowType,
            ["CorrelationId"] = correlationId
        };

        var metrics = new Dictionary<string, double>
        {
            ["DurationMs"] = duration.TotalMilliseconds
        };

        _telemetry.TrackEvent("AuthenticationSucceeded", properties, metrics);
        _logger.LogInformation("Authentication succeeded: Flow={AuthFlowType}, Duration={Duration}ms, CorrelationId={CorrelationId}",
            authFlowType, duration.TotalMilliseconds, correlationId);
    }

    public void TrackAuthenticationFailure(string authFlowType, string errorCode, string errorMessage, string correlationId)
    {
        var properties = new Dictionary<string, string>
        {
            ["AuthFlowType"] = authFlowType,
            ["ErrorCode"] = errorCode,
            ["CorrelationId"] = correlationId
        };

        _telemetry.TrackEvent("AuthenticationFailed", properties);
        _logger.LogError("Authentication failed: Flow={AuthFlowType}, Error={ErrorCode}, CorrelationId={CorrelationId}, Message={ErrorMessage}",
            authFlowType, errorCode, correlationId, errorMessage);

        // DO NOT log exception details if they contain tokens or secrets
    }

    public void TrackTokenRefresh(bool success, TimeSpan tokenAge)
    {
        var properties = new Dictionary<string, string>
        {
            ["RefreshResult"] = success ? "Success" : "Failure"
        };

        var metrics = new Dictionary<string, double>
        {
            ["TokenAgeMinutes"] = tokenAge.TotalMinutes
        };

        _telemetry.TrackEvent("TokenRefreshed", properties, metrics);
    }
}
```

### Structured Logging with Redaction
```csharp
public class StructuredLogger
{
    private readonly ILogger _logger;

    public void LogAuthenticationEvent(LogLevel level, string message, params object[] args)
    {
        // Sanitize arguments to remove any potential token values
        var sanitizedArgs = args.Select(RedactSensitiveData).ToArray();
        _logger.Log(level, message, sanitizedArgs);
    }

    private object RedactSensitiveData(object value)
    {
        if (value is string str)
        {
            // Redact anything that looks like a JWT (3 base64 segments separated by dots)
            if (Regex.IsMatch(str, @"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$"))
            {
                return "[REDACTED-TOKEN]";
            }

            // Redact client secrets (long base64 strings)
            if (str.Length > 32 && Regex.IsMatch(str, @"^[A-Za-z0-9+/=]+$"))
            {
                return "[REDACTED-SECRET]";
            }
        }

        return value;
    }
}
```

### Key Telemetry Metrics
- **Authentication Success Rate**: % of successful authentications
- **Authentication Latency**: P50, P90, P99 response times
- **Token Refresh Frequency**: Refreshes per hour
- **Token Lifetime Utilization**: Average % of token lifetime used before refresh
- **Permanent Failure Rate**: % of unrecoverable auth failures
- **Retry Count Distribution**: Histogram of retry attempts

### Alternatives Considered
- **Serilog**: More flexible but requires additional setup
- **Custom Logging**: Reinventing the wheel, missing correlation features
- **No Telemetry**: Blind operational visibility

### References
- [Application Insights for .NET](https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core)
- [ILogger Best Practices](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)

---

## Summary of Key Decisions

| **Area** | **Decision** | **Rationale** |
|----------|--------------|---------------|
| **OAuth Flow** | Authorization Code + PKCE (local), Client Credentials (cloud) | Most secure for each environment type |
| **Auth Library** | Azure.Identity + MSAL.NET | Industry-standard, multi-environment support |
| **Token Storage** | Windows Credential Manager, Azure Key Vault, GitHub Secrets | Environment-native, secure, isolated |
| **Token Refresh** | Proactive (5min before expiry) | Prevents service interruption |
| **Error Handling** | Exponential backoff with jitter, fail-fast on permanent errors | Resilient yet efficient |
| **App Registration** | Separate registrations for local/cloud | Clear security boundaries |
| **Telemetry** | Application Insights + ILogger<T> | Operational visibility with security |

---

## Next Steps

1. ✅ Phase 0 Complete: All research questions resolved
2. ➡️ Phase 1: Generate data model, contracts, and quickstart guide
3. ➡️ Phase 2: Generate task list from Phase 1 artifacts
4. ➡️ Phase 3-4: Implementation and validation

---

**Document Status**: ✅ Complete | **Last Updated**: October 6, 2025
