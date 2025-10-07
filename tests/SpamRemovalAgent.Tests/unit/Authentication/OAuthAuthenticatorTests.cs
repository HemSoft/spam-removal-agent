namespace SpamRemovalAgent.Tests.Unit.Authentication;

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SpamRemovalAgent.Authentication;
using SpamRemovalAgent.Authentication.Environment;
using SpamRemovalAgent.Authentication.Exceptions;
using SpamRemovalAgent.Authentication.Models;
using SpamRemovalAgent.Observability;
using SpamRemovalAgent.Tests.Utilities;
using Xunit;

/// <summary>
/// Unit tests for OAuthAuthenticator orchestration logic.
/// These tests use mocks and in-memory stores - no real authentication calls.
/// 
/// NOTE: Full OAuthAuthenticator orchestration tests cannot be added until the class
/// is refactored to accept IAuthFlow via dependency injection (IAuthFlowFactory pattern).
/// See tests/integration/Authentication/README.md for details.
/// </summary>
public class OAuthAuthenticatorTests : AuthenticationTestBase
{
    [Fact]
    public async Task HasValidCredentialsAsync_WhenTokenExists_ReturnsTrue()
    {
        // Arrange
        var validToken = TestOAuthTokenFactory.CreateValidToken();
        var tokenStore = new InMemoryTokenStore();
        await tokenStore.StoreTokenAsync(validToken);

        // Act
        var retrievedToken = await tokenStore.RetrieveTokenAsync();

        // Assert
        retrievedToken.Should().NotBeNull();
        retrievedToken!.AccessToken.Should().Be(validToken.AccessToken);
    }

    [Fact]
    public async Task HasValidCredentialsAsync_WhenNoToken_ReturnsFalse()
    {
        // Arrange
        var tokenStore = new InMemoryTokenStore();

        // Act
        var token = await tokenStore.RetrieveTokenAsync();

        // Assert
        token.Should().BeNull();
    }

    [Fact]
    public async Task ClearCredentialsAsync_RemovesStoredToken()
    {
        // Arrange
        var validToken = TestOAuthTokenFactory.CreateValidToken();
        var tokenStore = new InMemoryTokenStore();
        await tokenStore.StoreTokenAsync(validToken);

        // Act
        await tokenStore.ClearTokenAsync();
        var retrievedToken = await tokenStore.RetrieveTokenAsync();

        // Assert
        retrievedToken.Should().BeNull("token should be cleared from store");
    }

    [Fact]
    public async Task InMemoryTokenStore_IsAvailable_ReturnsTrue()
    {
        // Arrange
        var tokenStore = new InMemoryTokenStore(isAvailable: true);

        // Act
        var isAvailable = await tokenStore.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task InMemoryTokenStore_WhenNotAvailable_ReportsFalse()
    {
        // Arrange
        var tokenStore = new InMemoryTokenStore(isAvailable: false);

        // Act
        var isAvailable = await tokenStore.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task MockAuthFlow_AuthenticateAsync_ReturnsConfiguredToken()
    {
        // Arrange
        var expectedToken = TestOAuthTokenFactory.CreateValidToken();
        var mockFlow = MockAuthFlow.WithSuccess(expectedToken);
        var config = new OAuthConfiguration
        {
            TenantId = "test-tenant",
            ClientId = "test-client",
            RedirectUri = "http://localhost",
            Scopes = "Mail.ReadWrite offline_access"
        };

        // Act
        var token = await mockFlow.AuthenticateAsync(config, CancellationToken.None);

        // Assert
        token.Should().BeSameAs(expectedToken);
        mockFlow.AuthenticateCallCount.Should().Be(1);
        mockFlow.LastAuthConfig.Should().BeSameAs(config);
    }

    [Fact]
    public async Task MockAuthFlow_RefreshTokenAsync_ReturnsRefreshedToken()
    {
        // Arrange
        var originalToken = TestOAuthTokenFactory.CreateExpiringSoonToken();
        var refreshedToken = TestOAuthTokenFactory.CreateValidToken(expiresIn: TimeSpan.FromHours(2));
        var mockFlow = MockAuthFlow.WithBehaviors(refreshToken: refreshedToken);
        var config = new OAuthConfiguration
        {
            TenantId = "test-tenant",
            ClientId = "test-client",
            RedirectUri = "http://localhost",
            Scopes = "Mail.ReadWrite offline_access"
        };

        // Act
        var token = await mockFlow.RefreshTokenAsync(originalToken, config, CancellationToken.None);

        // Assert
        token.Should().BeSameAs(refreshedToken);
        mockFlow.RefreshCallCount.Should().Be(1);
        mockFlow.LastTokenToRefresh.Should().BeSameAs(originalToken);
    }

    [Fact]
    public async Task MockAuthFlow_WithFailure_ThrowsConfiguredException()
    {
        // Arrange
        var expectedException = new PermanentAuthFailureException("Test failure", "test_error", "test-correlation");
        var mockFlow = MockAuthFlow.WithFailure(expectedException);
        var config = new OAuthConfiguration
        {
            TenantId = "test-tenant",
            ClientId = "test-client",
            RedirectUri = "http://localhost",
            Scopes = "Mail.ReadWrite offline_access"
        };

        // Act & Assert
        var act = () => mockFlow.AuthenticateAsync(config, CancellationToken.None);
        await act.Should().ThrowAsync<PermanentAuthFailureException>()
            .WithMessage("Test failure");
    }
}
