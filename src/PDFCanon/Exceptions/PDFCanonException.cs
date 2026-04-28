using System;

namespace PDFCanon.Exceptions;

/// <summary>Base exception for all PDFCanon API errors.</summary>
public class PDFCanonException : Exception
{
    /// <summary>The request ID returned by the API, if available.</summary>
    public string? RequestId { get; }

    /// <summary>Initializes a new instance with the specified message and optional request ID.</summary>
    public PDFCanonException(string message, string? requestId = null)
        : base(message)
    {
        RequestId = requestId;
    }

    /// <summary>Initializes a new instance with the specified message, optional request ID, and inner exception.</summary>
    public PDFCanonException(string message, string? requestId, Exception? innerException)
        : base(message, innerException)
    {
        RequestId = requestId;
    }
}
