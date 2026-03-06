namespace PDFCanon.Exceptions;

/// <summary>Thrown when the API toolchain encounters a processing error (HTTP 5xx).</summary>
public sealed class ToolchainException : PDFCanonException
{
    /// <summary>Whether the error is considered transient and may succeed if retried.</summary>
    public bool Retryable { get; }

    public ToolchainException(string message, bool retryable, string? requestId = null)
        : base(message, requestId)
    {
        Retryable = retryable;
    }
}
