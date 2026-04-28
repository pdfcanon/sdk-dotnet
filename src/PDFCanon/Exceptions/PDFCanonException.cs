using System;

namespace PDFCanon.Exceptions;

/// <summary>Base exception for all PDFCanon API errors.</summary>
public class PDFCanonException : Exception
{
    /// <summary>The request ID returned by the API, if available.</summary>
    public string? RequestId { get; }

    public PDFCanonException(string message, string? requestId = null)
        : base(message)
    {
        RequestId = requestId;
    }

    public PDFCanonException(string message, string? requestId, Exception? innerException)
        : base(message, innerException)
    {
        RequestId = requestId;
    }
}
