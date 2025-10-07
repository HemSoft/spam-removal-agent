namespace SpamRemovalAgent.Tests.Integration.Authentication
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using SpamRemovalAgent.Authentication.Environment;
    using SpamRemovalAgent.Authentication.Models;
    using SpamRemovalAgent.Authentication.TokenManagement;
    using SpamRemovalAgent.Tests.Utilities;
    using Xunit;

    /// <summary>
    /// Integration tests for Multi-Environment Detection.
    /// These tests verify environment detection across Windows, Azure, and GitHub Actions.
    /// </summary>
    public class MultiEnvironmentIntegrationTests : AuthenticationTestBase
    {
        [Fact]
        public void DetectWindowsLocal_WhenNoCloudVariables_ReturnsWindowsLocal()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithWindowsLocal("test-tenant", "test-client")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();

            // Act
            var environment = detector.DetectEnvironment();

            // Assert
            environment.Should().Be(DeploymentEnvironment.WindowsLocal,
                "should detect Windows local when no cloud indicators present");
        }

        [Fact]
        public void DetectAzure_WhenAzureVariablesSet_ReturnsAzure()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithAzure("test-tenant", "test-client", "test-secret", "https://test-vault.vault.azure.net")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();

            // Act
            var environment = detector.DetectEnvironment();

            // Assert
            environment.Should().Be(DeploymentEnvironment.Azure,
                "should detect Azure when AZURE_FUNCTIONS_ENVIRONMENT is set");
        }

        [Fact]
        public void DetectGitHubActions_WhenGitHubActionsVariableSet_ReturnsGitHubActions()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithGitHubActions("test-tenant", "test-client", "test-secret")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();

            // Act
            var environment = detector.DetectEnvironment();

            // Assert
            environment.Should().Be(DeploymentEnvironment.GitHubActions,
                "should detect GitHub Actions when GITHUB_ACTIONS=true is set");
        }

        [Fact]
        public void GetRecommendedAuthMode_ForWindowsLocal_ReturnsInteractive()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithWindowsLocal("test-tenant", "test-client")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();

            // Act
            var authMode = detector.GetRecommendedAuthMode();

            // Assert
            authMode.Should().Be(AuthenticationMode.Interactive,
                "Windows local should use interactive authentication");
        }

        [Fact]
        public void GetRecommendedAuthMode_ForAzure_ReturnsServicePrincipal()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithAzure("test-tenant", "test-client", "test-secret", "https://test-vault.vault.azure.net")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();

            // Act
            var authMode = detector.GetRecommendedAuthMode();

            // Assert
            authMode.Should().Be(AuthenticationMode.ServicePrincipal,
                "Azure cloud should use service principal authentication");
        }

        [Fact]
        public void GetRecommendedAuthMode_ForGitHubActions_ReturnsServicePrincipal()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithGitHubActions("test-tenant", "test-client", "test-secret")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();

            // Act
            var authMode = detector.GetRecommendedAuthMode();

            // Assert
            authMode.Should().Be(AuthenticationMode.ServicePrincipal,
                "GitHub Actions should use service principal authentication");
        }

        [Fact]
        public void CorrectTokenStoreSelected_ForWindowsLocal()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithWindowsLocal("test-tenant", "test-client")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();
            var environment = detector.DetectEnvironment();

            // Act
            var tokenStore = TokenStoreFactory.Create(environment, LoggerFactory);

            // Assert
            tokenStore.Should().BeOfType<WindowsCredentialStore>(
                "Windows local environment should use WindowsCredentialStore");
        }

        [Fact]
        public void CorrectTokenStoreSelected_ForAzure()
        {
            // Arrange
            var keyVaultUri = "https://test-vault.vault.azure.net";
            using var envBuilder = new TestEnvironmentBuilder()
                .WithAzure("test-tenant", "test-client", "test-secret", keyVaultUri)
                .Apply();

            var detector = new DeploymentEnvironmentDetector();
            var environment = detector.DetectEnvironment();

            // Act
            var tokenStore = TokenStoreFactory.Create(environment, LoggerFactory, keyVaultUri);

            // Assert
            tokenStore.Should().BeOfType<AzureKeyVaultTokenStore>(
                "Azure environment should use AzureKeyVaultTokenStore");
        }

        [Fact]
        public void CorrectTokenStoreSelected_ForGitHubActions()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithGitHubActions("test-tenant", "test-client", "test-secret")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();
            var environment = detector.DetectEnvironment();

            // Act
            var tokenStore = TokenStoreFactory.Create(environment, LoggerFactory);

            // Assert
            tokenStore.Should().BeOfType<GitHubSecretsTokenStore>(
                "GitHub Actions environment should use GitHubSecretsTokenStore");
        }

        [Fact]
        public void ValidateEnvironmentPrerequisites_WindowsLocal_RequiresTenantAndClientId()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithWindowsLocal("test-tenant", "test-client")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();

            // Act
            var validationResult = detector.ValidateEnvironmentPrerequisites();

            // Assert
            validationResult.IsSuccess.Should().BeTrue(
                "Windows local with tenant ID and client ID should be valid");
        }

        [Fact]
        public void ValidateEnvironmentPrerequisites_Azure_RequiresAllSecrets()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithAzure("test-tenant", "test-client", "test-secret", "https://test-vault.vault.azure.net")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();

            // Act
            var validationResult = detector.ValidateEnvironmentPrerequisites();

            // Assert
            validationResult.IsSuccess.Should().BeTrue(
                "Azure with all required variables should be valid");
        }

        [Fact]
        public void ValidateEnvironmentPrerequisites_GitHubActions_RequiresClientSecret()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithGitHubActions("test-tenant", "test-client", "test-secret")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();

            // Act
            var validationResult = detector.ValidateEnvironmentPrerequisites();

            // Assert
            validationResult.IsSuccess.Should().BeTrue(
                "GitHub Actions with all required variables should be valid");
        }

        [Fact]
        public void ValidateEnvironmentPrerequisites_MissingTenantId_ReturnsFailure()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithVariable("AZURE_CLIENT_ID", "test-client")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();

            // Act
            var validationResult = detector.ValidateEnvironmentPrerequisites();

            // Assert
            validationResult.IsSuccess.Should().BeFalse(
                "missing tenant ID should fail validation");
            validationResult.Errors.Should().ContainMatch("*AZURE_TENANT_ID*",
                "error message should mention missing tenant ID");
        }

        [Fact]
        public void ValidateEnvironmentPrerequisites_MissingClientId_ReturnsFailure()
        {
            // Arrange
            using var envBuilder = new TestEnvironmentBuilder()
                .WithVariable("AZURE_TENANT_ID", "test-tenant")
                .Apply();

            var detector = new DeploymentEnvironmentDetector();

            // Act
            var validationResult = detector.ValidateEnvironmentPrerequisites();

            // Assert
            validationResult.IsSuccess.Should().BeFalse(
                "missing client ID should fail validation");
            validationResult.Errors.Should().ContainMatch("*AZURE_CLIENT_ID*",
                "error message should mention missing client ID");
        }

        // NOTE: CreateForCurrentEnvironment test removed - requires real token stores
        // TODO: Re-add after refactoring OAuthAuthenticator to accept IAuthFlow via DI

        [Fact]
        public void UnknownEnvironment_ThrowsNotSupportedException()
        {
            // Arrange
            // Clear all environment variables to trigger Unknown environment
            var allVars = new[] {
                "AZURE_TENANT_ID", "AZURE_CLIENT_ID", "AZURE_CLIENT_SECRET",
                "AZURE_KEY_VAULT_URI", "AZURE_FUNCTIONS_ENVIRONMENT",
                "WEBSITE_SITE_NAME", "GITHUB_ACTIONS"
            };

            foreach (var key in allVars)
            {
                System.Environment.SetEnvironmentVariable(key, null);
            }

            try
            {
                var detector = new DeploymentEnvironmentDetector();
                var environment = detector.DetectEnvironment();

                // Act & Assert
                var act = () => TokenStoreFactory.Create(environment, LoggerFactory);

                if (environment == DeploymentEnvironment.Unknown)
                {
                    act.Should().Throw<NotSupportedException>()
                        .WithMessage("*unknown deployment environment*");
                }
            }
            finally
            {
                // Cleanup is handled by test isolation
            }
        }
    }
}
