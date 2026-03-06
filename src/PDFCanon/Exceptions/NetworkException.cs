namespace PDFCanon.Exceptions;

/// <summary>Thrown when a network-level error occurs while communicating with the API.</summary>
public sealed class NetworkException : PDFCanonException
{
    public NetworkException(string message, Exception innerException)
        : base(message, requestId: null, innerException)
    {
    }
}
