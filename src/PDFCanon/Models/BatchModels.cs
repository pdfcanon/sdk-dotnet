using System.Text.Json.Serialization;

namespace PDFCanon.Models;

/// <summary>Represents a batch group for tracking bulk normalization workflows.</summary>
public sealed class BatchResponse
{
    [JsonPropertyName("batch_id")]
    public string? BatchId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("webhook_url")]
    public string? WebhookUrl { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("completed")]
    public int Completed { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }

    [JsonPropertyName("rejected")]
    public int Rejected { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }
}

/// <summary>Paginated list of batch groups.</summary>
public sealed class ListBatchesResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    [JsonPropertyName("items")]
    public List<BatchResponse>? Items { get; set; }
}

/// <summary>Represents a single submission entry within a batch.</summary>
public sealed class BatchSubmissionEntry
{
    [JsonPropertyName("submission_id")]
    public string? SubmissionId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("original_sha256")]
    public string? OriginalSha256 { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }
}

/// <summary>Paginated list of submissions within a batch group.</summary>
public sealed class ListBatchSubmissionsResponse
{
    [JsonPropertyName("batch_id")]
    public string? BatchId { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    [JsonPropertyName("items")]
    public List<BatchSubmissionEntry>? Items { get; set; }
}
