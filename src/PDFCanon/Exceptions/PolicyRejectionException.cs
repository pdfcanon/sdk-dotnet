namespace PDFCanon.Exceptions;

/// <summary>Thrown when the API rejects the submission due to a policy violation (HTTP 422).</summary>
public sealed class PolicyRejectionException : PDFCanonException
{
    /// <summary>The rejection code returned by the API.</summary>
    public string Code { get; }

    public PolicyRejectionException(string message, string code, string? requestId = null)
        : base(message, requestId)
    {
        Code = code;
    }
}
