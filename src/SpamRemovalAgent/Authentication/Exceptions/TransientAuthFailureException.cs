namespace SpamRemovalAgent.Authentication.Exceptions;

/// <summary>
/// Exception thrown when authentication fails due to transient issues (network, throttling, etc.).
/// Can be retried with exponential backoff.
/// </summary>
public class TransientAuthFailureException : Exception
{
    /// <summary>
    /// Number of retry attempts made before throwing this exception.
    /// </summary>
    public int RetryCount { get; }

    public TransientAuthFailureException(string message, int retryCount = 0)
        : base(message)
    {
        RetryCount = retryCount;
    }

    public TransientAuthFailureException(string message, Exception innerException, int retryCount = 0)
        : base(message, innerException)
    {
        RetryCount = retryCount;
    }
}
