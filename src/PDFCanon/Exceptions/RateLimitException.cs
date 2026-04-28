namespace PDFCanon.Exceptions;

/// <summary>Thrown when the API rate limit is exceeded (HTTP 429).</summary>
public sealed class RateLimitException : PDFCanonException
{
    /// <summary>Milliseconds to wait before retrying, as indicated by the Retry-After header.</summary>
    public int RetryAfterMs { get; }

    /// <summary>Initializes a new instance with the specified message, retry delay, and optional request ID.</summary>
    public RateLimitException(string message, int retryAfterMs, string? requestId = null)
        : base(message, requestId)
    {
        RetryAfterMs = retryAfterMs;
    }
}
