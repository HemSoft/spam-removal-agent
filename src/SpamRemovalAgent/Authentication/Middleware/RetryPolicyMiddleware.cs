namespace SpamRemovalAgent.Authentication.Middleware
{
    using System.Net;
    using System.Net.Sockets;
    using Microsoft.Extensions.Logging;
    using SpamRemovalAgent.Authentication.Exceptions;

    /// <summary>
    /// Implements exponential backoff retry logic for transient authentication failures.
    /// Retries operations with increasing delays: 2s, 4s, 8s, 16s, 32s (max 5 attempts).
    /// </summary>
    public sealed class RetryPolicyMiddleware : IRetryPolicy
    {
        private readonly ILogger<RetryPolicyMiddleware> _logger;
        private static readonly TimeSpan[] BackoffDelays =
        [
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(4),
            TimeSpan.FromSeconds(8),
            TimeSpan.FromSeconds(16),
            TimeSpan.FromSeconds(32)
        ];

        private static readonly Random JitterRandom = new();
        private const int MaxJitterMilliseconds = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicyMiddleware"/> class.
        /// </summary>
        /// <param name="logger">Logger for retry policy operations and diagnostics.</param>
        public RetryPolicyMiddleware(ILogger<RetryPolicyMiddleware> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<T> ExecuteWithRetryAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            var attemptNumber = 0;
            Exception? lastException = null;

            while (attemptNumber <= BackoffDelays.Length)
            {
                try
                {
                    if (attemptNumber > 0)
                    {
                        _logger.LogInformation("Retry attempt {AttemptNumber} of {MaxAttempts}",
                            attemptNumber, BackoffDelays.Length);
                    }

                    return await operation(cancellationToken);
                }
                catch (Exception ex) when (IsTransientError(ex) && attemptNumber < BackoffDelays.Length)
                {
                    lastException = ex;
                    var delay = GetRetryDelay(attemptNumber);

                    _logger.LogWarning(ex,
                        "Transient error on attempt {AttemptNumber}. Retrying after {DelaySeconds:F2} seconds. Error: {ErrorMessage}",
                        attemptNumber + 1, delay.TotalSeconds, ex.Message);

                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Retry cancelled during delay period.");
                        throw;
                    }

                    attemptNumber++;
                }
                catch (Exception ex) when (!IsTransientError(ex))
                {
                    _logger.LogError(ex, "Permanent error detected. Will not retry. Error: {ErrorMessage}", ex.Message);
                    throw;
                }
            }

            // Max retries exceeded
            _logger.LogError(lastException,
                "Maximum retry attempts ({MaxAttempts}) exceeded. Operation failed permanently.",
                BackoffDelays.Length);

            if (lastException is TransientAuthFailureException transientEx)
            {
                throw new TransientAuthFailureException(
                    $"Operation failed after {BackoffDelays.Length} retry attempts: {transientEx.Message}",
                    transientEx,
                    BackoffDelays.Length);
            }

            throw new TransientAuthFailureException(
                $"Operation failed after {BackoffDelays.Length} retry attempts: {lastException?.Message ?? "Unknown error"}",
                lastException!,
                BackoffDelays.Length);
        }

        /// <inheritdoc />
        public bool IsTransientError(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception switch
            {
                // Explicit transient authentication failure
                TransientAuthFailureException => true,

                // Permanent failures should never be retried
                PermanentAuthFailureException => false,
                AuthenticationRequiredException => false,
                TokenStoreUnavailableException => false,

                // Network-related transient errors
                SocketException => true,
                TimeoutException => true,
                TaskCanceledException => false, // User-initiated cancellation, not transient

                // HTTP-related transient errors
                HttpRequestException httpEx => IsTransientHttpError(httpEx),

                // Aggregate exceptions (check inner exceptions)
                AggregateException aggregateEx => aggregateEx.InnerExceptions.Any(IsTransientError),

                // Default: not transient
                _ => false
            };
        }

        /// <inheritdoc />
        public TimeSpan GetRetryDelay(int attemptNumber)
        {
            if (attemptNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attemptNumber), "Attempt number cannot be negative.");
            }

            if (attemptNumber >= BackoffDelays.Length)
            {
                // Return max delay if attempt number exceeds array bounds
                return BackoffDelays[^1];
            }

            var baseDelay = BackoffDelays[attemptNumber];
            var jitter = TimeSpan.FromMilliseconds(JitterRandom.Next(0, MaxJitterMilliseconds));

            return baseDelay + jitter;
        }

        private static bool IsTransientHttpError(HttpRequestException httpException)
        {
            // Check status code if available
            if (httpException.StatusCode.HasValue)
            {
                return httpException.StatusCode.Value switch
                {
                    HttpStatusCode.RequestTimeout => true,      // 408
                    HttpStatusCode.TooManyRequests => true,     // 429
                    HttpStatusCode.InternalServerError => true, // 500
                    HttpStatusCode.BadGateway => true,          // 502
                    HttpStatusCode.ServiceUnavailable => true,  // 503
                    HttpStatusCode.GatewayTimeout => true,      // 504
                    _ => false
                };
            }

            // Check exception message for transient indicators
            var message = httpException.Message.ToLowerInvariant();
            return message.Contains("timeout") ||
                   message.Contains("network") ||
                   message.Contains("connection") ||
                   message.Contains("temporarily unavailable");
        }
    }
}
