using SpamRemovalAgent.Authentication.Models;

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
