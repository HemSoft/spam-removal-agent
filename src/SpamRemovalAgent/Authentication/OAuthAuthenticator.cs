namespace SpamRemovalAgent.Authentication;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpamRemovalAgent.Authentication.Environment;
using SpamRemovalAgent.Authentication.Exceptions;
using SpamRemovalAgent.Authentication.Flows;
using SpamRemovalAgent.Authentication.Models;
using SpamRemovalAgent.Authentication.TokenManagement;
using SpamRemovalAgent.Observability;

/// <summary>
/// Main orchestrator for OAuth 2.0 authentication with Microsoft Graph.
/// Handles token acquisition, refresh, and storage across different deployment environments.
/// Thread-safe and suitable for singleton injection.
/// </summary>
public sealed class OAuthAuthenticator : IOAuthAuthenticator
{
    private readonly ILogger<OAuthAuthenticator> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ITokenStore _tokenStore;
    private readonly IDeploymentEnvironmentDetector _environmentDetector;
    private readonly IAuthenticationTelemetry _telemetry;
    private readonly OAuthConfiguration _configuration;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private const int TokenExpirationBufferMinutes = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthAuthenticator"/> class.
    /// </summary>
    /// <param name="configuration">Configuration root for loading OAuth settings.</param>
    /// <param name="loggerFactory">Logger factory for creating flow-specific loggers.</param>
    /// <param name="tokenStore">Token storage implementation for the current environment.</param>
    /// <param name="environmentDetector">Environment detection service.</param>
    /// <param name="telemetry">Telemetry tracking service.</param>
    public OAuthAuthenticator(
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        ITokenStore tokenStore,
        IDeploymentEnvironmentDetector environmentDetector,
        IAuthenticationTelemetry telemetry)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<OAuthAuthenticator>();
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        _environmentDetector = environmentDetector ?? throw new ArgumentNullException(nameof(environmentDetector));
        _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));

        ArgumentNullException.ThrowIfNull(configuration);
        _configuration = OAuthConfiguration.LoadFromEnvironment(configuration);

        var environment = _environmentDetector.DetectEnvironment();
        var validationResult = _configuration.Validate(environment);

        if (!validationResult.IsSuccess)
        {
            var errors = string.Join(", ", validationResult.Errors);
            _logger.LogError("OAuth configuration validation failed: {Errors}", errors);
            throw new InvalidOperationException($"OAuth configuration is invalid: {errors}");
        }

        _logger.LogInformation("OAuthAuthenticator initialized for {Environment} environment",
            environment);
    }

    /// <inheritdoc />
    public async Task<string> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        var environment = _environmentDetector.DetectEnvironment();
        var authMode = _configuration.AuthMode ?? _environmentDetector.GetRecommendedAuthMode();
        var authFlowType = authMode.ToString();

        _logger.LogInformation("Starting authentication flow: {AuthFlowType} for {Environment}",
            authFlowType, environment);

        var startTime = DateTimeOffset.UtcNow;
        _telemetry.TrackAuthenticationStart(authFlowType, environment);

        try
        {
            var authFlow = CreateAuthFlow(authMode);
            var token = await authFlow.AuthenticateAsync(_configuration, cancellationToken);

            // Store the token securely
            await _tokenStore.StoreTokenAsync(token, cancellationToken);

            var duration = DateTimeOffset.UtcNow - startTime;
            _telemetry.TrackAuthenticationSuccess(authFlowType, duration, token.CorrelationId ?? "none");

            _logger.LogInformation("Authentication successful. Token expires at: {ExpiresAt} UTC. Duration: {Duration}ms. CorrelationId: {CorrelationId}",
                token.ExpiresAt, duration.TotalMilliseconds, token.CorrelationId);

            return token.AccessToken;
        }
        catch (Exception ex) when (ex is PermanentAuthFailureException or TransientAuthFailureException)
        {
            var correlationId = (ex as PermanentAuthFailureException)?.CorrelationId ?? "none";
            var errorCode = (ex as PermanentAuthFailureException)?.ErrorCode ?? "transient_failure";

            _telemetry.TrackAuthenticationFailure(authFlowType, errorCode, ex.Message, correlationId);

            _logger.LogError(ex, "Authentication failed with error code: {ErrorCode}. CorrelationId: {CorrelationId}",
                errorCode, correlationId);

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetValidAccessTokenAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        // Use lock to prevent concurrent refresh operations
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            var token = await _tokenStore.RetrieveTokenAsync(cancellationToken);

            // No token stored - authenticate first
            if (token == null)
            {
                _logger.LogInformation("No token found in storage. Initiating new authentication.");
                return await AuthenticateAsync(cancellationToken);
            }

            // Check if token needs refresh
            var bufferTime = TimeSpan.FromMinutes(TokenExpirationBufferMinutes);
            var needsRefresh = forceRefresh || token.IsExpiringSoon(bufferTime);

            if (!needsRefresh)
            {
                _logger.LogDebug("Returning cached token. Expires at: {ExpiresAt} UTC", token.ExpiresAt);
                return token.AccessToken;
            }

            // Token is expiring soon or force refresh requested
            _logger.LogInformation("Token refresh required. Expiring: {IsExpiring}, ForceRefresh: {ForceRefresh}. Expires at: {ExpiresAt} UTC",
                token.IsExpiringSoon(bufferTime), forceRefresh, token.ExpiresAt);

            var tokenAge = DateTimeOffset.UtcNow - token.ExpiresAt.AddHours(-1); // Approximate age
            var startTime = DateTimeOffset.UtcNow;

            try
            {
                var environment = _environmentDetector.DetectEnvironment();
                var authMode = _configuration.AuthMode ?? _environmentDetector.GetRecommendedAuthMode();
                var authFlow = CreateAuthFlow(authMode);

                var refreshedToken = await authFlow.RefreshTokenAsync(token, _configuration, cancellationToken);

                // Store the refreshed token
                await _tokenStore.StoreTokenAsync(refreshedToken, cancellationToken);

                var duration = DateTimeOffset.UtcNow - startTime;
                _telemetry.TrackTokenRefresh(success: true, tokenAge);

                _logger.LogInformation("Token refresh successful. New token expires at: {ExpiresAt} UTC. Duration: {Duration}ms",
                    refreshedToken.ExpiresAt, duration.TotalMilliseconds);

                return refreshedToken.AccessToken;
            }
            catch (AuthenticationRequiredException ex)
            {
                _telemetry.TrackTokenRefresh(success: false, tokenAge);

                _logger.LogWarning(ex, "Token refresh requires re-authentication. Clearing stored credentials.");

                // Clear invalid credentials
                await ClearCredentialsAsync(cancellationToken);

                throw;
            }
            catch (TransientAuthFailureException ex)
            {
                _telemetry.TrackTokenRefresh(success: false, tokenAge);

                _logger.LogWarning(ex, "Token refresh failed with transient error. Will retry.");

                throw;
            }
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task ClearCredentialsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing stored authentication credentials.");

        var startTime = DateTimeOffset.UtcNow;
        var environment = _environmentDetector.DetectEnvironment();

        try
        {
            await _tokenStore.ClearTokenAsync(cancellationToken);

            var duration = DateTimeOffset.UtcNow - startTime;
            _telemetry.TrackTokenStoreOperation("clear", environment, success: true, duration);

            _logger.LogInformation("Credentials cleared successfully. Duration: {Duration}ms", duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            _telemetry.TrackTokenStoreOperation("clear", environment, success: false, duration);

            _logger.LogError(ex, "Failed to clear credentials. Duration: {Duration}ms", duration.TotalMilliseconds);

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasValidCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var environment = _environmentDetector.DetectEnvironment();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            var token = await _tokenStore.RetrieveTokenAsync(cancellationToken);

            if (token == null)
            {
                _logger.LogDebug("No credentials found in storage.");
                return false;
            }

            // Check if token is expired (without buffer)
            var isValid = !token.IsExpiringSoon(TimeSpan.Zero);

            var duration = DateTimeOffset.UtcNow - startTime;
            _telemetry.TrackTokenStoreOperation("retrieve", environment, success: true, duration);

            _logger.LogDebug("Credentials check: {IsValid}. Token expires at: {ExpiresAt} UTC", isValid, token.ExpiresAt);

            return isValid;
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            _telemetry.TrackTokenStoreOperation("retrieve", environment, success: false, duration);

            _logger.LogWarning(ex, "Failed to check credentials validity. Duration: {Duration}ms", duration.TotalMilliseconds);

            return false;
        }
    }

    private IAuthFlow CreateAuthFlow(AuthenticationMode authMode)
    {
        return authMode switch
        {
            AuthenticationMode.Interactive => new InteractiveAuthFlow(
                _loggerFactory.CreateLogger<InteractiveAuthFlow>()),
            AuthenticationMode.ServicePrincipal => new ServicePrincipalAuthFlow(
                _loggerFactory.CreateLogger<ServicePrincipalAuthFlow>()),
            _ => throw new NotSupportedException($"Authentication mode not supported: {authMode}")
        };
    }
}
