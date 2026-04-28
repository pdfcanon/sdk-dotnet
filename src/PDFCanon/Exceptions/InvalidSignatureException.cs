namespace PDFCanon.Exceptions;

/// <summary>Thrown when a webhook signature fails verification.</summary>
public sealed class InvalidSignatureException : PDFCanonException
{
    /// <summary>Initializes a new instance with the specified message.</summary>
    public InvalidSignatureException(string message)
        : base(message, requestId: null)
    {
    }
}
