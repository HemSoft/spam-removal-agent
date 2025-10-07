# Testing Strategy for Spam Removal Agent

## Overview

This document explains the testing architecture and strategy for the authentication components.

## Testing Philosophy

### Unit Tests vs Integration Tests

**Unit Tests** (`tests/SpamRemovalAgent.Tests/unit/`):
- Test business logic in isolation
- Use mocks and in-memory implementations
- Fast, deterministic, no external dependencies
- Run on every build, no skipping
- **NEVER** touch real systems (Windows Credential Manager, Azure AD, etc.)

**Integration Tests** (`tests/SpamRemovalAgent.Tests/integration/`):
- Test integration with real external systems
- Require actual Azure AD configuration
- May modify system state (Windows Credential Manager)
- Marked with `Skip` attribute by default
- Only run manually when explicitly enabled
- Require environment variables like `RUN_INTEGRATION_TESTS=true`

**Contract Tests** (`tests/SpamRemovalAgent.Tests/integration/contracts/`):
- Test data model contracts and validation logic
- Can run on every build
- Use TestEnvironmentBuilder to set up isolated environments
- Do not make real authentication calls

## Test Utilities

### InMemoryTokenStore
**Location**: `tests/SpamRemovalAgent.Tests/utilities/InMemoryTokenStore.cs`

In-memory token storage for unit tests. Does not require Windows Credential Manager or any external dependencies.

**Usage**:
```csharp
var tokenStore = new InMemoryTokenStore();
await tokenStore.StoreTokenAsync(token);
var retrieved = await tokenStore.RetrieveTokenAsync();
```

### MockAuthFlow
**Location**: `tests/SpamRemovalAgent.Tests/utilities/MockAuthFlow.cs`

Mock authentication flow that returns pre-configured tokens without making real MSAL calls.

**Usage**:
```csharp
// Success scenario
var mockFlow = MockAuthFlow.WithSuccess(expectedToken);
var token = await mockFlow.AuthenticateAsync(config, CancellationToken.None);

// Failure scenario
var mockFlow = MockAuthFlow.WithFailure(new PermanentAuthFailureException(...));

// Track calls
Assert.Equal(1, mockFlow.AuthenticateCallCount);
Assert.Same(config, mockFlow.LastAuthConfig);
```

### TestEnvironmentBuilder
**Location**: `tests/SpamRemovalAgent.Tests/utilities/TestEnvironmentBuilder.cs`

Builder for setting up test environment variables with automatic cleanup.

**Usage**:
```csharp
using var _ = new TestEnvironmentBuilder()
    .WithWindowsLocal("tenant-id", "client-id")
    .Apply();

// Environment variables are set for test duration
// Automatically restored when disposed
```

### TestOAuthTokenFactory
**Location**: `tests/SpamRemovalAgent.Tests/utilities/TestOAuthTokenFactory.cs`

Factory for creating realistic test tokens.

**Usage**:
```csharp
var validToken = TestOAuthTokenFactory.CreateValidToken();
var expiredToken = TestOAuthTokenFactory.CreateExpiredToken();
var expiringSoonToken = TestOAuthTokenFactory.CreateExpiringSoonToken();
```

## Current Architectural Issue

### Problem: OAuthAuthenticator Cannot Be Properly Unit Tested

The `OAuthAuthenticator` class currently creates `IAuthFlow` instances internally:

```csharp
private IAuthFlow CreateAuthFlow(AuthenticationMode authMode)
{
    return authMode switch
    {
        AuthenticationMode.Interactive => new InteractiveAuthFlow(...),
        AuthenticationMode.ServicePrincipal => new ServicePrincipalAuthFlow(...),
        _ => throw new NotSupportedException(...)
    };
}
```

**This means**:
- Unit tests cannot inject mock auth flows
- Tests that call `AuthenticateAsync()` will trigger real MSAL authentication
- Cannot test orchestration logic without hitting real systems

### Solution: Dependency Injection Refactoring

**Option 1: Factory Pattern** (Recommended)
```csharp
public interface IAuthFlowFactory
{
    IAuthFlow CreateAuthFlow(AuthenticationMode mode);
}

public class OAuthAuthenticator
{
    private readonly IAuthFlowFactory _authFlowFactory;

    public OAuthAuthenticator(..., IAuthFlowFactory authFlowFactory)
    {
        _authFlowFactory = authFlowFactory;
    }

    private IAuthFlow CreateAuthFlow(AuthenticationMode mode)
    {
        return _authFlowFactory.CreateAuthFlow(mode);
    }
}

// In tests:
var mockFactory = new Mock<IAuthFlowFactory>();
mockFactory.Setup(f => f.CreateAuthFlow(It.IsAny<AuthenticationMode>()))
           .Returns(MockAuthFlow.WithSuccess(expectedToken));
```

**Option 2: Direct Injection**
```csharp
public class OAuthAuthenticator
{
    public OAuthAuthenticator(..., IAuthFlow authFlow)
    {
        _authFlow = authFlow;
    }
}
```

**TODO**: Implement one of these refactorings to enable proper unit testing of OAuthAuthenticator.

## Running Tests

### Run All Unit Tests
```bash
dotnet test --filter "FullyQualifiedName~Unit"
```

### Run Contract Tests
```bash
dotnet test --filter "FullyQualifiedName~Contracts"
```

### Run Integration Tests (Manual Only)
```bash
# Set required environment variables
export TEST_AZURE_TENANT_ID="your-tenant-id"
export TEST_AZURE_CLIENT_ID="your-client-id"
export RUN_INTEGRATION_TESTS="true"

# Remove Skip attributes or use test explorer to run specific tests
dotnet test --filter "FullyQualifiedName~Integration"
```

## Test Categories

| Category | External Dependencies | Runs on CI/CD | Skip Attribute |
|----------|----------------------|---------------|----------------|
| Unit Tests | None (mocks only) | ✅ Yes | ❌ No |
| Contract Tests | Environment variables only | ✅ Yes | ❌ No |
| Integration Tests | Real systems (Azure AD, Credential Manager) | ❌ No | ✅ Yes |

## Best Practices

1. **Always use mocks for unit tests** - Never instantiate real `OAuthAuthenticator`, `WindowsCredentialStore`, etc.
2. **Use `InMemoryTokenStore` for token storage tests** - Fast and no side effects
3. **Use `MockAuthFlow` for authentication flow tests** - No MSAL, no browser prompts
4. **Mark integration tests with `Skip` attribute** - Prevents accidental execution
5. **Use `TestEnvironmentBuilder` with `using` statements** - Ensures cleanup
6. **Document why tests are skipped** - Help future maintainers understand intent

## Common Testing Mistakes

❌ **BAD** - Creates real authenticator in unit test:
```csharp
[Fact]
public async Task TestAuthentication()
{
    var authenticator = new OAuthAuthenticator(...); // Will try to authenticate!
    await authenticator.AuthenticateAsync(); // Opens browser!
}
```

✅ **GOOD** - Uses mocks:
```csharp
[Fact]
public async Task TestTokenStorage()
{
    var tokenStore = new InMemoryTokenStore();
    var token = TestOAuthTokenFactory.CreateValidToken();
    await tokenStore.StoreTokenAsync(token);
    var retrieved = await tokenStore.RetrieveTokenAsync();
    Assert.Equal(token.AccessToken, retrieved.AccessToken);
}
```

❌ **BAD** - Integration test without Skip:
```csharp
[Fact] // No Skip attribute!
public async Task TestRealAuthentication()
{
    var tokenStore = new WindowsCredentialStore(); // Real system!
    // ...
}
```

✅ **GOOD** - Integration test properly marked:
```csharp
[Fact(Skip = "Integration test - requires Windows Credential Manager")]
public async Task TestRealAuthentication()
{
    var tokenStore = new WindowsCredentialStore();
    // ...
}
```

## Future Improvements

1. **Refactor OAuthAuthenticator for testability** - Implement `IAuthFlowFactory` pattern
2. **Add more unit tests for OAuthAuthenticator orchestration** - Once refactoring is complete
3. **Create MockMsalClientApplication** - Mock MSAL library directly for auth flow tests
4. **Add test data builders** - Fluent API for complex test scenarios
5. **Separate integration test project** - `SpamRemovalAgent.IntegrationTests` with own config
