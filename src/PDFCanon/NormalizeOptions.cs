namespace PDFCanon;

/// <summary>Options for normalizing a PDF.</summary>
public sealed class NormalizeOptions
{
    /// <summary>Whether to remove annotations from the PDF.</summary>
    public bool RemoveAnnotations { get; set; }

    /// <summary>Optional webhook URL to receive completion notification.</summary>
    public string? WebhookUrl { get; set; }

    /// <summary>Optional idempotency key to deduplicate requests.</summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Policy to apply when the PDF contains digital signatures.
    /// <list type="bullet">
    ///   <item><term>reject</term><description>Return a rejection with code <c>SIGNED_PDF</c> (default).</description></item>
    ///   <item><term>strip</term><description>Remove signatures and continue normalization.</description></item>
    ///   <item><term>preserve</term><description>Return the original file unchanged.</description></item>
    /// </list>
    /// </summary>
    public string? SignedPdfPolicy { get; set; }
}
