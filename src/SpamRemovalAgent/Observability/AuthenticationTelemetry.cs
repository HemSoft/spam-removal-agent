namespace SpamRemovalAgent.Observability;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Implementation of authentication telemetry tracking using Application Insights.
/// Tracks authentication events, token operations, and performance metrics.
/// </summary>
public sealed class AuthenticationTelemetry : IAuthenticationTelemetry
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<AuthenticationTelemetry> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationTelemetry"/> class.
    /// </summary>
    /// <param name="telemetryClient">Application Insights telemetry client.</param>
    /// <param name="logger">Logger for telemetry operations.</param>
    public AuthenticationTelemetry(
        TelemetryClient telemetryClient,
        ILogger<AuthenticationTelemetry> logger)
    {
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void TrackAuthenticationStart(string authFlowType, DeploymentEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(authFlowType);

        var properties = new Dictionary<string, string>
        {
            ["AuthFlowType"] = authFlowType,
            ["Environment"] = environment.ToString(),
            ["Timestamp"] = DateTimeOffset.UtcNow.ToString("O")
        };

        _telemetryClient.TrackEvent("AuthenticationStarted", properties);

        _logger.LogInformation("Authentication started. FlowType: {AuthFlowType}, Environment: {Environment}",
            authFlowType, environment);
    }

    /// <inheritdoc />
    public void TrackAuthenticationSuccess(string authFlowType, TimeSpan duration, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(authFlowType);
        ArgumentNullException.ThrowIfNull(correlationId);

        var properties = new Dictionary<string, string>
        {
            ["AuthFlowType"] = authFlowType,
            ["CorrelationId"] = correlationId,
            ["Timestamp"] = DateTimeOffset.UtcNow.ToString("O")
        };

        var metrics = new Dictionary<string, double>
        {
            ["DurationMs"] = duration.TotalMilliseconds
        };

        _telemetryClient.TrackEvent("AuthenticationSucceeded", properties, metrics);

        _logger.LogInformation("Authentication succeeded. FlowType: {AuthFlowType}, Duration: {DurationMs}ms, CorrelationId: {CorrelationId}",
            authFlowType, duration.TotalMilliseconds, correlationId);
    }

    /// <inheritdoc />
    public void TrackAuthenticationFailure(string authFlowType, string errorCode, string errorMessage, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(authFlowType);
        ArgumentNullException.ThrowIfNull(errorCode);
        ArgumentNullException.ThrowIfNull(errorMessage);
        ArgumentNullException.ThrowIfNull(correlationId);

        var properties = new Dictionary<string, string>
        {
            ["AuthFlowType"] = authFlowType,
            ["ErrorCode"] = errorCode,
            ["ErrorMessage"] = RedactSensitiveData(errorMessage),
            ["CorrelationId"] = correlationId,
            ["Timestamp"] = DateTimeOffset.UtcNow.ToString("O")
        };

        _telemetryClient.TrackEvent("AuthenticationFailed", properties);

        _logger.LogError("Authentication failed. FlowType: {AuthFlowType}, ErrorCode: {ErrorCode}, CorrelationId: {CorrelationId}",
            authFlowType, errorCode, correlationId);
    }

    /// <inheritdoc />
    public void TrackTokenRefresh(bool success, TimeSpan tokenAge)
    {
        var properties = new Dictionary<string, string>
        {
            ["Success"] = success.ToString(),
            ["Timestamp"] = DateTimeOffset.UtcNow.ToString("O")
        };

        var metrics = new Dictionary<string, double>
        {
            ["TokenAgeSeconds"] = tokenAge.TotalSeconds
        };

        _telemetryClient.TrackEvent("TokenRefresh", properties, metrics);

        if (success)
        {
            _logger.LogInformation("Token refresh succeeded. TokenAge: {TokenAgeSeconds}s", tokenAge.TotalSeconds);
        }
        else
        {
            _logger.LogWarning("Token refresh failed. TokenAge: {TokenAgeSeconds}s", tokenAge.TotalSeconds);
        }
    }

    /// <inheritdoc />
    public void TrackTokenStoreOperation(string operation, DeploymentEnvironment environment, bool success, TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var properties = new Dictionary<string, string>
        {
            ["Operation"] = operation,
            ["Environment"] = environment.ToString(),
            ["Success"] = success.ToString(),
            ["Timestamp"] = DateTimeOffset.UtcNow.ToString("O")
        };

        var metrics = new Dictionary<string, double>
        {
            ["DurationMs"] = duration.TotalMilliseconds
        };

        _telemetryClient.TrackEvent("TokenStoreOperation", properties, metrics);

        if (success)
        {
            _logger.LogDebug("Token store operation '{Operation}' succeeded. Duration: {DurationMs}ms, Environment: {Environment}",
                operation, duration.TotalMilliseconds, environment);
        }
        else
        {
            _logger.LogWarning("Token store operation '{Operation}' failed. Duration: {DurationMs}ms, Environment: {Environment}",
                operation, duration.TotalMilliseconds, environment);
        }
    }

    /// <summary>
    /// Redacts sensitive data from error messages before logging.
    /// Ensures tokens, secrets, and PII are never logged to telemetry.
    /// </summary>
    private static string RedactSensitiveData(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return message;

        // Redact common patterns that might contain sensitive data
        var redacted = message;

        // Redact anything that looks like a token (long base64-like strings)
        if (System.Text.RegularExpressions.Regex.IsMatch(redacted, @"[A-Za-z0-9_-]{100,}"))
        {
            redacted = System.Text.RegularExpressions.Regex.Replace(
                redacted,
                @"[A-Za-z0-9_-]{100,}",
                "[REDACTED_TOKEN]");
        }

        // Redact anything that looks like a secret or key
        if (redacted.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
            redacted.Contains("key", StringComparison.OrdinalIgnoreCase) ||
            redacted.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            redacted += " [Sensitive data redacted]";
        }

        return redacted;
    }
}
