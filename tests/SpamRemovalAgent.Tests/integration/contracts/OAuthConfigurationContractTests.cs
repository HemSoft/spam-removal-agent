using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SpamRemovalAgent.Authentication.Models;
using SpamRemovalAgent.Tests.Utilities;

namespace SpamRemovalAgent.Tests.Integration.Contracts;

/// <summary>
/// Contract tests for OAuthConfiguration data model.
/// These tests verify environment variable loading and validation behavior.
/// </summary>
public class OAuthConfigurationContractTests
{
    [Fact]
    public void LoadFromEnvironment_WhenVariablesSet_LoadsCorrectly()
    {
        // Arrange
        var envBuilder = new TestEnvironmentBuilder()
            .WithWindowsLocal("tenant123", "client456");

        using var _ = envBuilder.Apply();

        var configuration = new ConfigurationBuilder().Build();

        // Act
        var config = OAuthConfiguration.LoadFromEnvironment(configuration);

        // Assert
        config.TenantId.Should().Be("tenant123");
        config.ClientId.Should().Be("client456");
        config.RedirectUri.Should().Be("http://localhost");
        config.Scopes.Should().Be("Mail.ReadWrite offline_access");
    }

    [Fact]
    public void LoadFromEnvironment_WhenMissingTenantId_ThrowsException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Clear environment variables
        System.Environment.SetEnvironmentVariable("AZURE_TENANT_ID", null);
        System.Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", null);

        // Act
        var act = () => OAuthConfiguration.LoadFromEnvironment(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*AZURE_TENANT_ID*");
    }

    [Fact]
    public void Validate_ForWindowsLocal_RequiresRedirectUri()
    {
        // Arrange
        var config = new OAuthConfiguration
        {
            TenantId = "tenant123",
            ClientId = "client456",
            RedirectUri = null, // Missing
            Scopes = "Mail.ReadWrite offline_access"
        };

        // Act
        var result = config.Validate(DeploymentEnvironment.WindowsLocal);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RedirectUri", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ForWindowsLocal_RequiresOfflineAccessScope()
    {
        // Arrange
        var config = new OAuthConfiguration
        {
            TenantId = "tenant123",
            ClientId = "client456",
            RedirectUri = "http://localhost",
            Scopes = "Mail.ReadWrite" // Missing offline_access
        };

        // Act
        var result = config.Validate(DeploymentEnvironment.WindowsLocal);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("offline_access", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ForAzure_RequiresClientSecretAndKeyVault()
    {
        // Arrange
        var config = new OAuthConfiguration
        {
            TenantId = "tenant123",
            ClientId = "client456",
            ClientSecret = null, // Missing
            KeyVaultUri = null, // Missing
            Scopes = "https://graph.microsoft.com/.default"
        };

        // Act
        var result = config.Validate(DeploymentEnvironment.Azure);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ClientSecret", StringComparison.OrdinalIgnoreCase));
        result.Errors.Should().Contain(e => e.Contains("KeyVaultUri", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ForAzure_RequiresDefaultScope()
    {
        // Arrange
        var config = new OAuthConfiguration
        {
            TenantId = "tenant123",
            ClientId = "client456",
            ClientSecret = "secret",
            KeyVaultUri = "https://kv.vault.azure.net/",
            Scopes = "Mail.ReadWrite" // Wrong scope for service principal
        };

        // Act
        var result = config.Validate(DeploymentEnvironment.Azure);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains(".default", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ForGitHubActions_RequiresClientSecret()
    {
        // Arrange
        var config = new OAuthConfiguration
        {
            TenantId = "tenant123",
            ClientId = "client456",
            ClientSecret = null, // Missing
            Scopes = "https://graph.microsoft.com/.default"
        };

        // Act
        var result = config.Validate(DeploymentEnvironment.GitHubActions);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ClientSecret", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ValidConfiguration_ReturnsSuccess()
    {
        // Arrange
        var config = new OAuthConfiguration
        {
            TenantId = "tenant123",
            ClientId = "client456",
            RedirectUri = "http://localhost",
            Scopes = "Mail.ReadWrite offline_access"
        };

        // Act
        var result = config.Validate(DeploymentEnvironment.WindowsLocal);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
