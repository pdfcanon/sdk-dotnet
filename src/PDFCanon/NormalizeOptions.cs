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
}
