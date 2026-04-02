using System.Text.Json.Serialization;

namespace PDFCanon.Models;

/// <summary>Represents the full response returned by the PDFCanon API for a normalization job.</summary>
public sealed class NormalizeResponse
{
    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; set; }

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("submissionId")]
    public string? SubmissionId { get; set; }

    [JsonPropertyName("processingTimeMs")]
    public long ProcessingTimeMs { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("original")]
    public OriginalInfo? Original { get; set; }

    [JsonPropertyName("normalized")]
    public NormalizedInfo? Normalized { get; set; }

    [JsonPropertyName("security")]
    public SecurityInfo? Security { get; set; }

    [JsonPropertyName("validation")]
    public ValidationInfo? Validation { get; set; }

    [JsonPropertyName("tamperAnalysis")]
    public TamperAnalysisInfo? TamperAnalysis { get; set; }

    [JsonPropertyName("warnings")]
    public List<WarningInfo>? Warnings { get; set; }

    [JsonPropertyName("failure")]
    public FailureInfo? Failure { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("healthScore")]
    public int? HealthScore { get; set; }

    [JsonPropertyName("healthScoreDeductions")]
    public List<HealthScoreDeduction>? HealthScoreDeductions { get; set; }
}

public sealed class OriginalInfo
{
    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }

    [JsonPropertyName("sizeBytes")]
    public long SizeBytes { get; set; }
}

public sealed class NormalizedInfo
{
    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }

    [JsonPropertyName("sizeBytes")]
    public long SizeBytes { get; set; }

    [JsonPropertyName("pdfVersion")]
    public string? PdfVersion { get; set; }

    [JsonPropertyName("linearized")]
    public bool Linearized { get; set; }

    [JsonPropertyName("downloadUrl")]
    public string? DownloadUrl { get; set; }

    [JsonPropertyName("contentHash")]
    public string? ContentHash { get; set; }
}

public sealed class SecurityInfo
{
    [JsonPropertyName("javascriptRemoved")]
    public bool JavascriptRemoved { get; set; }

    [JsonPropertyName("openActionsRemoved")]
    public bool OpenActionsRemoved { get; set; }

    [JsonPropertyName("embeddedFilesRemoved")]
    public bool EmbeddedFilesRemoved { get; set; }

    [JsonPropertyName("richMediaRemoved")]
    public bool RichMediaRemoved { get; set; }

    [JsonPropertyName("launchActionsRemoved")]
    public bool LaunchActionsRemoved { get; set; }

    [JsonPropertyName("incrementalUpdatesRemoved")]
    public bool IncrementalUpdatesRemoved { get; set; }

    [JsonPropertyName("acroformFlattened")]
    public bool AcroformFlattened { get; set; }

    [JsonPropertyName("annotationsRemoved")]
    public bool AnnotationsRemoved { get; set; }

    [JsonPropertyName("encryptedInput")]
    public bool EncryptedInput { get; set; }

    [JsonPropertyName("digitalSignaturesDetected")]
    public bool DigitalSignaturesDetected { get; set; }

    [JsonPropertyName("digitalSignaturesRemoved")]
    public bool DigitalSignaturesRemoved { get; set; }

    [JsonPropertyName("signatureCount")]
    public int SignatureCount { get; set; }

    [JsonPropertyName("signatureVerificationResults")]
    public List<SignatureVerificationResult>? SignatureVerificationResults { get; set; }

    [JsonPropertyName("overallSignatureStatus")]
    public string? OverallSignatureStatus { get; set; }
}

/// <summary>Cryptographic verification result for a single digital signature field.</summary>
public sealed class SignatureVerificationResult
{
    [JsonPropertyName("fieldName")]
    public string? FieldName { get; set; }

    [JsonPropertyName("signerName")]
    public string? SignerName { get; set; }

    [JsonPropertyName("signedAt")]
    public DateTimeOffset? SignedAt { get; set; }

    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    [JsonPropertyName("certificateExpired")]
    public bool CertificateExpired { get; set; }

    [JsonPropertyName("certificateChainTrusted")]
    public bool? CertificateChainTrusted { get; set; }

    [JsonPropertyName("timestampPresent")]
    public bool TimestampPresent { get; set; }

    [JsonPropertyName("timestampValid")]
    public bool? TimestampValid { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public sealed class ValidationInfo
{
    [JsonPropertyName("xrefRebuilt")]
    public bool XrefRebuilt { get; set; }

    [JsonPropertyName("objectStreamsRegenerated")]
    public bool ObjectStreamsRegenerated { get; set; }

    [JsonPropertyName("brokenReferencesDetected")]
    public bool BrokenReferencesDetected { get; set; }

    [JsonPropertyName("nonEmbeddedFontsDetected")]
    public bool NonEmbeddedFontsDetected { get; set; }

    [JsonPropertyName("pdfaDeclared")]
    public bool PdfaDeclared { get; set; }

    [JsonPropertyName("pdfaLevel")]
    public string? PdfaLevel { get; set; }

    [JsonPropertyName("pdfaPreserved")]
    public bool PdfaPreserved { get; set; }

    [JsonPropertyName("pdfaCompliant")]
    public bool PdfaCompliant { get; set; }

    /// <summary>Whether veraPDF validation was performed and the document passed. Null when not performed.</summary>
    [JsonPropertyName("verapdfValidated")]
    public bool? VerapdfValidated { get; set; }

    /// <summary>Specific errors reported by veraPDF. Empty or null when validation passed or was not performed.</summary>
    [JsonPropertyName("verapdfErrors")]
    public List<string>? VerapdfErrors { get; set; }
}

public sealed class WarningInfo
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public sealed class FailureInfo
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("stage")]
    public string? Stage { get; set; }
}

/// <summary>Tamper analysis results from the pre-repair structural analysis.</summary>
public sealed class TamperAnalysisInfo
{
    [JsonPropertyName("riskLevel")]
    public string? RiskLevel { get; set; }

    [JsonPropertyName("anomaliesDetected")]
    public int AnomaliesDetected { get; set; }

    [JsonPropertyName("anomalies")]
    public List<TamperAnomalyInfo>? Anomalies { get; set; }
}

/// <summary>A single structural anomaly detected during tamper analysis.</summary>
public sealed class TamperAnomalyInfo
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("severity")]
    public string? Severity { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }
}

/// <summary>A single deduction applied to the document health score.</summary>
public sealed class HealthScoreDeduction
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("points")]
    public int Points { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }
}
