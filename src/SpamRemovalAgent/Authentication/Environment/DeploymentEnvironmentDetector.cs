using SpamRemovalAgent.Authentication.Models;

namespace SpamRemovalAgent.Authentication.Environment;

/// <summary>
/// Detects the current deployment environment to determine authentication strategy.
/// </summary>
public class DeploymentEnvironmentDetector : IDeploymentEnvironmentDetector
{
    /// <summary>
    /// Detects the current deployment environment.
    /// Checks for Azure-specific environment variables, GitHub Actions context, or defaults to Windows local.
    /// </summary>
    public DeploymentEnvironment DetectEnvironment()
    {
        // Check for GitHub Actions first (most specific)
        if (System.Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
        {
            return DeploymentEnvironment.GitHubActions;
        }

        // Check for Azure environment indicators
        var azureVars = new[]
        {
            "AZURE_FUNCTIONS_ENVIRONMENT",
            "WEBSITE_SITE_NAME",
            "WEBSITE_INSTANCE_ID"
        };

        if (azureVars.Any(v => !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(v))))
        {
            return DeploymentEnvironment.Azure;
        }

        // Default to Windows local
        return DeploymentEnvironment.WindowsLocal;
    }

    /// <summary>
    /// Gets the recommended authentication mode for the current environment.
    /// </summary>
    public AuthenticationMode GetRecommendedAuthMode()
    {
        var environment = DetectEnvironment();

        return environment switch
        {
            DeploymentEnvironment.WindowsLocal => AuthenticationMode.Interactive,
            DeploymentEnvironment.Azure => AuthenticationMode.ServicePrincipal,
            DeploymentEnvironment.GitHubActions => AuthenticationMode.ServicePrincipal,
            _ => AuthenticationMode.Interactive
        };
    }

    /// <summary>
    /// Validates that the current environment has all required prerequisites for authentication.
    /// </summary>
    public ValidationResult ValidateEnvironmentPrerequisites()
    {
        var environment = DetectEnvironment();
        var errors = new List<string>();

        switch (environment)
        {
            case DeploymentEnvironment.WindowsLocal:
                // Check for required variables
                if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AZURE_TENANT_ID")))
                    errors.Add("AZURE_TENANT_ID environment variable is required");

                if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")))
                    errors.Add("AZURE_CLIENT_ID environment variable is required");
                break;

            case DeploymentEnvironment.Azure:
                // Check for required variables
                if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AZURE_TENANT_ID")))
                    errors.Add("AZURE_TENANT_ID environment variable is required");

                if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")))
                    errors.Add("AZURE_CLIENT_ID environment variable is required");

                if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")))
                    errors.Add("AZURE_CLIENT_SECRET environment variable is required");

                if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI")))
                    errors.Add("AZURE_KEY_VAULT_URI environment variable is required");
                break;

            case DeploymentEnvironment.GitHubActions:
                // Check for required variables
                if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AZURE_TENANT_ID")))
                    errors.Add("AZURE_TENANT_ID environment variable is required");

                if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")))
                    errors.Add("AZURE_CLIENT_ID environment variable is required");

                if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")))
                    errors.Add("AZURE_CLIENT_SECRET environment variable is required");
                break;
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
