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
