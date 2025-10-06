# Contract: OAuth 2.0 Authentication System

**Date**: October 6, 2025
**Branch**: `002-oauth-2-0`
**Related Spec**: [../spec.md](../spec.md)
**Related Plan**: [../plan.md](../plan.md)

---

## Overview

This document defines the external contracts (interfaces and public APIs) for the OAuth 2.0 authentication system. All public interfaces must remain stable and backward-compatible.

---

## 1. Core Authentication Interface

### IOAuthAuthenticator

Primary interface for OAuth 2.0 authentication operations.

```csharp
namespace SpamRemovalAgent.Authentication;

/// <summary>
/// Primary interface for OAuth 2.0 authentication with Microsoft Graph.
/// Supports both interactive and service principal authentication flows.
/// Thread-safe and suitable for singleton injection.
/// </summary>
public interface IOAuthAuthenticator
{
    /// <summary>
    /// Authenticates and obtains an access token for Microsoft Graph.
    /// Uses interactive flow (browser) for Windows local, service principal for cloud.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Valid access token for Graph API calls</returns>
    /// <exception cref="PermanentAuthFailureException">Authentication failed and should not retry</exception>
    /// <exception cref="TransientAuthFailureException">Authentication failed but may retry</exception>
    /// <exception cref="TokenStoreUnavailableException">Secure storage mechanism not available</exception>
    Task<string> AuthenticateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a valid access token, refreshing proactively if needed.
    /// This method handles token expiration and refresh automatically.
    /// </summary>
    /// <param name="forceRefresh">If true, forces token refresh even if not expired</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Valid access token for Graph API calls</returns>
    Task<string> GetValidAccessTokenAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all stored authentication credentials.
    /// Used for sign-out or when refresh token is permanently invalid.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    Task ClearCredentialsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if valid authentication credentials are currently stored.
    /// Does not validate token with server, only checks local storage.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>True if credentials exist and appear valid, false otherwise</returns>
    Task<bool> HasValidCredentialsAsync(CancellationToken cancellationToken = default);
}
```

**Contract Tests** (must pass):
```csharp
[Fact]
public async Task AuthenticateAsync_WhenCalled_ReturnsNonEmptyToken()
{
    // Arrange
    var authenticator = CreateAuthenticator();

    // Act
    var token = await authenticator.AuthenticateAsync();

    // Assert
    Assert.NotNull(token);
    Assert.NotEmpty(token);
    Assert.Matches(@"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$", token); // JWT format
}

[Fact]
public async Task GetValidAccessTokenAsync_WhenTokenExpired_RefreshesAutomatically()
{
    // Arrange
    var authenticator = CreateAuthenticator();
    await authenticator.AuthenticateAsync();

    // Simulate token expiration
    AdvanceTime(TimeSpan.FromHours(1));

    // Act
    var token = await authenticator.GetValidAccessTokenAsync();

    // Assert
    Assert.NotNull(token);
}

[Fact]
public async Task ClearCredentialsAsync_WhenCalled_RemovesStoredTokens()
{
    // Arrange
    var authenticator = CreateAuthenticator();
    await authenticator.AuthenticateAsync();

    // Act
    await authenticator.ClearCredentialsAsync();

    // Assert
    var hasCredentials = await authenticator.HasValidCredentialsAsync();
    Assert.False(hasCredentials);
}
```

---

## 2. Token Storage Interface

### ITokenStore

Abstract interface for environment-specific token storage.

```csharp
namespace SpamRemovalAgent.Authentication.TokenManagement;

/// <summary>
/// Interface for secure OAuth token storage.
/// Implementations must provide encryption at rest and thread-safe operations.
/// </summary>
public interface ITokenStore
{
    /// <summary>
    /// Retrieves the stored OAuth token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>OAuth token if found, null if no token stored</returns>
    /// <exception cref="TokenStoreUnavailableException">Storage mechanism unavailable</exception>
    Task<OAuthToken?> RetrieveTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the OAuth token securely.
    /// Overwrites any existing token.
    /// </summary>
    /// <param name="token">OAuth token to store</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <exception cref="TokenStoreUnavailableException">Storage mechanism unavailable</exception>
    /// <exception cref="ArgumentNullException">Token is null</exception>
    Task StoreTokenAsync(OAuthToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the stored OAuth token.
    /// No-op if no token is stored.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    Task ClearTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the token store is available and operational.
    /// Called at startup to validate storage prerequisites.
    /// </summary>
    /// <returns>True if storage is available, false otherwise</returns>
    Task<bool> IsAvailableAsync();
}
```

**Contract Tests** (must pass for all implementations):
```csharp
[Theory]
[InlineData(typeof(WindowsCredentialStore))]
[InlineData(typeof(AzureKeyVaultTokenStore))]
[InlineData(typeof(GitHubSecretsTokenStore))]
public async Task StoreAndRetrieve_RoundTrip_PreservesAllFields(Type storeType)
{
    // Arrange
    var store = CreateTokenStore(storeType);
    var originalToken = new OAuthToken
    {
        AccessToken = "eyJ0eXAi...",
        RefreshToken = "AwABAA...",
        ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
        Scope = "Mail.ReadWrite offline_access",
        CorrelationId = Guid.NewGuid().ToString()
    };

    // Act
    await store.StoreTokenAsync(originalToken);
    var retrievedToken = await store.RetrieveTokenAsync();

    // Assert
    Assert.NotNull(retrievedToken);
    Assert.Equal(originalToken.AccessToken, retrievedToken.AccessToken);
    Assert.Equal(originalToken.RefreshToken, retrievedToken.RefreshToken);
    Assert.Equal(originalToken.Scope, retrievedToken.Scope);
}

[Theory]
[InlineData(typeof(WindowsCredentialStore))]
[InlineData(typeof(AzureKeyVaultTokenStore))]
[InlineData(typeof(GitHubSecretsTokenStore))]
public async Task ClearToken_WhenCalled_RemovesStoredToken(Type storeType)
{
    // Arrange
    var store = CreateTokenStore(storeType);
    await store.StoreTokenAsync(CreateValidToken());

    // Act
    await store.ClearTokenAsync();

    // Assert
    var token = await store.RetrieveTokenAsync();
    Assert.Null(token);
}
```

---

## 3. Environment Detection Interface

### IDeploymentEnvironmentDetector

```csharp
namespace SpamRemovalAgent.Authentication.Environment;

/// <summary>
/// Detects the current deployment environment to determine authentication strategy.
/// </summary>
public interface IDeploymentEnvironmentDetector
{
    /// <summary>
    /// Detects the current deployment environment.
    /// Checks for Azure-specific environment variables, GitHub Actions context, or defaults to Windows local.
    /// </summary>
    /// <returns>Detected deployment environment</returns>
    DeploymentEnvironment DetectEnvironment();

    /// <summary>
    /// Gets the recommended authentication mode for the current environment.
    /// </summary>
    /// <returns>Interactive for Windows local, ServicePrincipal for cloud</returns>
    AuthenticationMode GetRecommendedAuthMode();

    /// <summary>
    /// Validates that the current environment has all required prerequisites for authentication.
    /// </summary>
    /// <returns>Validation result with any missing prerequisites</returns>
    ValidationResult ValidateEnvironmentPrerequisites();
}
```

**Contract Tests**:
```csharp
[Fact]
public void DetectEnvironment_WhenRunningOnWindows_ReturnsWindowsLocal()
{
    // Arrange
    var detector = new DeploymentEnvironmentDetector();
    ClearAllCloudEnvironmentVariables();

    // Act
    var environment = detector.DetectEnvironment();

    // Assert
    Assert.Equal(DeploymentEnvironment.WindowsLocal, environment);
}

[Fact]
public void DetectEnvironment_WhenAzureEnvironmentVariablesSet_ReturnsAzure()
{
    // Arrange
    Environment.SetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT", "Production");
    var detector = new DeploymentEnvironmentDetector();

    // Act
    var environment = detector.DetectEnvironment();

    // Assert
    Assert.Equal(DeploymentEnvironment.Azure, environment);
}

[Fact]
public void DetectEnvironment_WhenGitHubActionsVariableSet_ReturnsGitHubActions()
{
    // Arrange
    Environment.SetEnvironmentVariable("GITHUB_ACTIONS", "true");
    var detector = new DeploymentEnvironmentDetector();

    // Act
    var environment = detector.DetectEnvironment();

    // Assert
    Assert.Equal(DeploymentEnvironment.GitHubActions, environment);
}
```

---

## 4. Retry Policy Interface

### IRetryPolicy

```csharp
namespace SpamRemovalAgent.Authentication.Middleware;

/// <summary>
/// Interface for retry policies with exponential backoff and circuit breaker.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Executes an operation with retry logic.
    /// Transient failures trigger retries with exponential backoff.
    /// Permanent failures fail immediately without retry.
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if an exception represents a transient failure eligible for retry.
    /// </summary>
    /// <param name="exception">Exception to evaluate</param>
    /// <returns>True if transient, false if permanent</returns>
    bool IsTransientError(Exception exception);

    /// <summary>
    /// Gets the retry delay for a specific attempt number.
    /// </summary>
    /// <param name="attemptNumber">1-based attempt number</param>
    /// <returns>Delay duration with jitter</returns>
    TimeSpan GetRetryDelay(int attemptNumber);
}
```

---

## 5. Telemetry Interface

### IAuthenticationTelemetry

```csharp
namespace SpamRemovalAgent.Observability;

/// <summary>
/// Interface for authentication telemetry and logging.
/// </summary>
public interface IAuthenticationTelemetry
{
    /// <summary>
    /// Tracks the start of an authentication attempt.
    /// </summary>
    void TrackAuthenticationStart(string authFlowType, DeploymentEnvironment environment);

    /// <summary>
    /// Tracks successful authentication.
    /// </summary>
    void TrackAuthenticationSuccess(string authFlowType, TimeSpan duration, string correlationId);

    /// <summary>
    /// Tracks failed authentication.
    /// </summary>
    void TrackAuthenticationFailure(string authFlowType, string errorCode, string errorMessage, string correlationId);

    /// <summary>
    /// Tracks token refresh operations.
    /// </summary>
    void TrackTokenRefresh(bool success, TimeSpan tokenAge);

    /// <summary>
    /// Tracks token store operations (read/write/clear).
    /// </summary>
    void TrackTokenStoreOperation(string operation, DeploymentEnvironment environment, bool success, TimeSpan duration);
}
```

---

## Contract Validation Requirements

### Backward Compatibility Rules

1. **Interface Changes**:
   - ✅ Adding new optional parameters with defaults
   - ✅ Adding new methods to interfaces (with default implementations in base classes)
   - ❌ Removing methods or parameters
   - ❌ Changing method signatures
   - ❌ Changing return types

2. **Data Model Changes**:
   - ✅ Adding new optional properties with nullable types
   - ✅ Adding validation rules (fail new invalid data, but don't break existing valid data)
   - ❌ Removing properties
   - ❌ Changing property types
   - ❌ Making optional properties required

3. **Exception Changes**:
   - ✅ Adding new exception types inheriting from existing base exceptions
   - ❌ Changing exception hierarchy
   - ❌ Removing exception types that external callers may catch

### Contract Test Coverage

All public interfaces must have:
- ✅ Unit tests for each public method
- ✅ Integration tests for end-to-end scenarios
- ✅ Contract tests validating JSON serialization/deserialization
- ✅ Null/empty input validation tests
- ✅ Concurrent access tests (thread safety)

---

## API Stability Guarantees

| **Component** | **Stability** | **Breaking Change Policy** |
|---------------|---------------|----------------------------|
| **IOAuthAuthenticator** | Stable | Major version increment required |
| **ITokenStore** | Stable | Major version increment required |
| **OAuthToken** | Stable | Major version increment required |
| **OAuthConfiguration** | Stable | Major version increment required |
| **DeploymentEnvironment** | Stable | New enum values OK, removing values requires major version |
| **AuthenticationResult** | Stable | Adding properties OK, removing requires major version |
| **Internal implementations** | Unstable | Can change without notice (not part of public API) |

---

**Document Status**: ✅ Complete | **Last Updated**: October 6, 2025
