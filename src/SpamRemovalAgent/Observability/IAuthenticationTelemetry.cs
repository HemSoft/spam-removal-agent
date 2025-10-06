using SpamRemovalAgent.Authentication.Models;

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
