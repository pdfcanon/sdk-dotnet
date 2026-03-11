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

    /// <summary>
    /// Policy to apply when a PDF/A-compliant document is detected.
    /// <list type="bullet">
    ///   <item><term>preserve</term><description>Allow safe structural operations while preserving <c>pdfaid</c> XMP namespace elements (default).</description></item>
    ///   <item><term>normalize_anyway</term><description>Perform full normalization including XMP stripping. Adds warning <c>PDF_A_COMPLIANCE_REMOVED</c>.</description></item>
    /// </list>
    /// </summary>
    public string? PdfaPolicy { get; set; }

    /// <summary>
    /// Whether to linearize (web-optimize) the output PDF.
    /// When <c>null</c> (the default), the server default of <c>true</c> is used.
    /// Set to <c>false</c> to skip linearization.
    /// </summary>
    public bool? Linearize { get; set; }

    /// <summary>
    /// Optional batch group ID to link this submission to a batch created via
    /// <see cref="PDFCanonClient.CreateBatchAsync"/>. When <c>null</c> (the default),
    /// the submission is standalone.
    /// </summary>
    public Guid? BatchId { get; set; }
}
