namespace SpamRemovalAgent.Authentication.Middleware;

/// <summary>
/// Interface for retry policies with exponential backoff and circuit breaker.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Executes an operation with retry logic.
    /// Transient failures trigger retries with exponential backoff.
    /// Permanent failures fail immediately without retry.
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if an exception represents a transient failure eligible for retry.
    /// </summary>
    /// <param name="exception">Exception to evaluate</param>
    /// <returns>True if transient, false if permanent</returns>
    bool IsTransientError(Exception exception);

    /// <summary>
    /// Gets the retry delay for a specific attempt number.
    /// </summary>
    /// <param name="attemptNumber">1-based attempt number</param>
    /// <returns>Delay duration with jitter</returns>
    TimeSpan GetRetryDelay(int attemptNumber);
}
