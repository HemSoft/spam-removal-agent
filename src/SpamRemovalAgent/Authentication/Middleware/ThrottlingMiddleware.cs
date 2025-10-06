namespace SpamRemovalAgent.Authentication.Middleware
{
    using System.Globalization;
    using System.Net;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Middleware for handling HTTP 429 (Too Many Requests) throttling responses.
    /// Respects the Retry-After header and waits before retrying the request.
    /// </summary>
    public sealed class ThrottlingMiddleware
    {
        private readonly ILogger<ThrottlingMiddleware> _logger;
        private const int MaxThrottleRetries = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottlingMiddleware"/> class.
        /// </summary>
        /// <param name="logger">Logger for throttling operations and diagnostics.</param>
        public ThrottlingMiddleware(ILogger<ThrottlingMiddleware> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sends an HTTP request with automatic throttling handling.
        /// If a 429 response is received, waits for the duration specified in the Retry-After header and retries.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for sending the request.</param>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        /// <exception cref="HttpRequestException">Thrown when the request fails after maximum throttle retries.</exception>
        public async Task<HttpResponseMessage> SendWithThrottlingAsync(
            HttpClient httpClient,
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(request);

            var attemptNumber = 0;

            while (attemptNumber <= MaxThrottleRetries)
            {
                try
                {
                    var response = await httpClient.SendAsync(request, cancellationToken);

                    if (response.StatusCode == HttpStatusCode.TooManyRequests && attemptNumber < MaxThrottleRetries)
                    {
                        var retryAfter = GetRetryAfterDelay(response);

                        _logger.LogWarning(
                            "HTTP 429 (Too Many Requests) received on attempt {AttemptNumber}. " +
                            "Waiting {DelaySeconds:F2} seconds before retry. " +
                            "Request: {Method} {Uri}",
                            attemptNumber + 1,
                            retryAfter.TotalSeconds,
                            request.Method,
                            request.RequestUri);

                        try
                        {
                            await Task.Delay(retryAfter, cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation("Throttle retry cancelled during delay period.");
                            throw;
                        }

                        attemptNumber++;

                        // Clone the request for retry (original request may have been consumed)
                        request = await CloneHttpRequestAsync(request);
                        continue;
                    }

                    return response;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("HTTP request was cancelled.");
                    throw;
                }
            }

            _logger.LogError(
                "Maximum throttle retries ({MaxRetries}) exceeded for request: {Method} {Uri}",
                MaxThrottleRetries,
                request.Method,
                request.RequestUri);

            throw new HttpRequestException(
                $"HTTP 429 (Too Many Requests) persisted after {MaxThrottleRetries} retry attempts. " +
                "The service may be experiencing high load or rate limits may be exceeded.");
        }

        private TimeSpan GetRetryAfterDelay(HttpResponseMessage response)
        {
            // Try to read Retry-After header
            if (response.Headers.RetryAfter != null)
            {
                // Retry-After can be either a delay in seconds or an HTTP-date
                if (response.Headers.RetryAfter.Delta.HasValue)
                {
                    var delay = response.Headers.RetryAfter.Delta.Value;
                    _logger.LogDebug("Retry-After header specifies delay: {DelaySeconds} seconds", delay.TotalSeconds);
                    return delay;
                }

                if (response.Headers.RetryAfter.Date.HasValue)
                {
                    var retryAt = response.Headers.RetryAfter.Date.Value.UtcDateTime;
                    var delay = retryAt - DateTime.UtcNow;

                    if (delay < TimeSpan.Zero)
                    {
                        // Date is in the past, use minimum delay
                        delay = TimeSpan.FromSeconds(1);
                    }

                    _logger.LogDebug("Retry-After header specifies date: {RetryAt} UTC (delay: {DelaySeconds} seconds)",
                        retryAt, delay.TotalSeconds);

                    return delay;
                }
            }

            // No Retry-After header or unable to parse - use default backoff
            var defaultDelay = TimeSpan.FromSeconds(60);
            _logger.LogDebug("No valid Retry-After header found. Using default delay: {DelaySeconds} seconds",
                defaultDelay.TotalSeconds);

            return defaultDelay;
        }

        private static async Task<HttpRequestMessage> CloneHttpRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            // Copy headers
            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Copy content if present
            if (request.Content != null)
            {
                var contentBytes = await request.Content.ReadAsByteArrayAsync();
                clone.Content = new ByteArrayContent(contentBytes);

                // Copy content headers
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return clone;
        }
    }
}
