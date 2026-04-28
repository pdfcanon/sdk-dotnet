namespace PDFCanon.Exceptions;

/// <summary>Thrown when the API key is missing or invalid (HTTP 401).</summary>
public sealed class AuthenticationException : PDFCanonException
{
    /// <summary>Initializes a new instance with the specified message and optional request ID.</summary>
    public AuthenticationException(string message, string? requestId = null)
        : base(message, requestId)
    {
    }
}
