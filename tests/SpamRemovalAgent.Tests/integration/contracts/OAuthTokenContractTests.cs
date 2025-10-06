using FluentAssertions;
using SpamRemovalAgent.Authentication.Models;
using SpamRemovalAgent.Tests.Utilities;

namespace SpamRemovalAgent.Tests.Integration.Contracts;

/// <summary>
/// Contract tests for OAuthToken data model.
/// These tests verify JSON serialization and validation behavior.
/// </summary>
public class OAuthTokenContractTests
{
    [Fact]
    public void Serialize_ThenDeserialize_PreservesAllProperties()
    {
        // Arrange
        var originalToken = TestOAuthTokenFactory.CreateValidToken();

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(originalToken);
        var deserializedToken = System.Text.Json.JsonSerializer.Deserialize<OAuthToken>(json);

        // Assert
        deserializedToken.Should().NotBeNull();
        deserializedToken!.AccessToken.Should().Be(originalToken.AccessToken);
        deserializedToken.RefreshToken.Should().Be(originalToken.RefreshToken);
        deserializedToken.ExpiresAt.Should().BeCloseTo(originalToken.ExpiresAt, TimeSpan.FromSeconds(1));
        deserializedToken.Scope.Should().Be(originalToken.Scope);
        deserializedToken.TokenType.Should().Be(originalToken.TokenType);
        deserializedToken.CorrelationId.Should().Be(originalToken.CorrelationId);
    }

    [Fact]
    public void Deserialize_ValidJson_CreatesToken()
    {
        // Arrange
        var json = """
        {
            "access_token": "eyJ0.eyJ1.sig",
            "refresh_token": "refresh123",
            "expires_at": "2025-10-06T15:30:00Z",
            "scope": "Mail.ReadWrite offline_access",
            "token_type": "Bearer",
            "correlation_id": "12345678-1234-1234-1234-123456789012"
        }
        """;

        // Act
        var token = System.Text.Json.JsonSerializer.Deserialize<OAuthToken>(json);

        // Assert
        token.Should().NotBeNull();
        token!.AccessToken.Should().Contain(".");
        token.RefreshToken.Should().Be("refresh123");
        token.Scope.Should().Be("Mail.ReadWrite offline_access");
    }

    [Fact]
    public void Validate_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var expiredToken = TestOAuthTokenFactory.CreateExpiredToken();

        // Act
        var result = expiredToken.Validate();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("expired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_MissingScope_ReturnsFailure()
    {
        // Arrange
        var token = TestOAuthTokenFactory.CreateValidToken(scope: "User.Read");

        // Act
        var result = token.Validate();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Mail.ReadWrite", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var token = TestOAuthTokenFactory.CreateValidToken();

        // Act
        var result = token.Validate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void IsExpiringSoon_WithDefaultBuffer_ReturnsTrueWhenWithin5Minutes()
    {
        // Arrange
        var token = TestOAuthTokenFactory.CreateExpiringSoonToken();

        // Act
        var isExpiring = token.IsExpiringSoon();

        // Assert
        isExpiring.Should().BeTrue();
    }

    [Fact]
    public void IsExpiringSoon_WithCustomBuffer_RespectsBufferTime()
    {
        // Arrange
        var token = TestOAuthTokenFactory.CreateValidToken(expiresIn: TimeSpan.FromMinutes(10));

        // Act
        var isExpiring = token.IsExpiringSoon(bufferTime: TimeSpan.FromMinutes(15));

        // Assert
        isExpiring.Should().BeTrue();
    }
}
