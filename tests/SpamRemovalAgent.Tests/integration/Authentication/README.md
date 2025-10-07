# Integration Tests - Authentication

## ⚠️ Current Status: Temporarily Removed

The integration tests for `InteractiveAuthFlow`, `ServicePrincipalAuthFlow`, and token refresh scenarios have been **temporarily removed** because they cannot be properly tested with the current architecture.

## Why Were They Removed?

### The Architectural Problem

`OAuthAuthenticator` creates `IAuthFlow` instances internally using the `new` keyword:

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

**This makes it impossible to:**
- Mock authentication flows in unit tests
- Test orchestration logic without triggering real MSAL calls
- Run tests in CI/CD without Azure AD configuration
- Test error handling and edge cases reliably

### What Happened During Testing

When tests tried to instantiate `OAuthAuthenticator`:
1. It created real `InteractiveAuthFlow` instances
2. These triggered Microsoft MSAL library calls
3. MSAL opened browser windows for authentication
4. Test processes hung waiting for user input
5. File locks prevented rebuilding
6. Tests became unusable

## What's Working Now

✅ **Contract Tests** (`integration/contracts/`):
- Configuration validation
- Token model contracts
- Environment detection
- No authentication calls required

✅ **Unit Tests** (`unit/Authentication/`):
- Token storage with `InMemoryTokenStore`
- Mock flow behavior with `MockAuthFlow`
- Isolated component testing

## The Solution: Architectural Refactoring

### Recommended Approach: Factory Pattern

```csharp
public interface IAuthFlowFactory
{
    IAuthFlow CreateAuthFlow(AuthenticationMode mode);
}

public class AuthFlowFactory : IAuthFlowFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public AuthFlowFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public IAuthFlow CreateAuthFlow(AuthenticationMode mode)
    {
        return mode switch
        {
            AuthenticationMode.Interactive =>
                new InteractiveAuthFlow(_loggerFactory.CreateLogger<InteractiveAuthFlow>()),
            AuthenticationMode.ServicePrincipal =>
                new ServicePrincipalAuthFlow(_loggerFactory.CreateLogger<ServicePrincipalAuthFlow>()),
            _ => throw new NotSupportedException($"Mode not supported: {mode}")
        };
    }
}

// In OAuthAuthenticator constructor:
public OAuthAuthenticator(
    IConfiguration configuration,
    ILoggerFactory loggerFactory,
    ITokenStore tokenStore,
    IDeploymentEnvironmentDetector environmentDetector,
    IAuthenticationTelemetry telemetry,
    IAuthFlowFactory authFlowFactory)  // <-- Add this
{
    _authFlowFactory = authFlowFactory;
    // ...
}
```

### Then Testing Becomes Possible

```csharp
[Fact]
public async Task AuthenticateAsync_StoresTokenSuccessfully()
{
    // Arrange
    var expectedToken = TestOAuthTokenFactory.CreateValidToken();
    var mockFactory = new Mock<IAuthFlowFactory>();
    mockFactory
        .Setup(f => f.CreateAuthFlow(AuthenticationMode.Interactive))
        .Returns(MockAuthFlow.WithSuccess(expectedToken));

    var tokenStore = new InMemoryTokenStore();
    var authenticator = new OAuthAuthenticator(
        config, loggerFactory, tokenStore,
        envDetector, telemetry, mockFactory.Object);

    // Act
    var accessToken = await authenticator.AuthenticateAsync();

    // Assert
    accessToken.Should().Be(expectedToken.AccessToken);
    var storedToken = await tokenStore.RetrieveTokenAsync();
    storedToken.Should().NotBeNull();
}
```

## What To Do Next

1. **Implement `IAuthFlowFactory`** - Add the factory pattern to `OAuthAuthenticator`
2. **Update DI Registration** - Register factory in Program.cs
3. **Re-add Integration Tests** - With proper mocking support
4. **Add Real Integration Tests** - In a separate test project that requires manual Azure AD setup

## Files Removed

- `InteractiveAuthFlowIntegrationTests.cs` - Hung on browser prompts
- `ServicePrincipalAuthFlowIntegrationTests.cs` - Required real Azure credentials
- `TokenRefreshIntegrationTests.cs` - Triggered real MSAL calls

These will be restored after refactoring.

## References

- Mock utilities: `tests/SpamRemovalAgent.Tests/utilities/MockAuthFlow.cs`
- Test token factory: `tests/SpamRemovalAgent.Tests/utilities/TestOAuthTokenFactory.cs`
- In-memory storage: `tests/SpamRemovalAgent.Tests/utilities/InMemoryTokenStore.cs`
- Testing strategy: `tests/TESTING_STRATEGY.md`
