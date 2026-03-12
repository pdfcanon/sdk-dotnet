# PDFCanon .NET SDK

Official .NET SDK for the [PDFCanon](https://pdfcanon.com) API — normalize, sanitize, and validate PDF files.

## Installation

```bash
dotnet add package PDFCanon
```

## Authentication

Obtain an API key from the [PDFCanon portal](https://pdfcanon.com/portal/api-keys). Pass it to the client constructor or set the `PDFCANON_API_KEY` environment variable.

The SDK sends the key as `X-Api-Key: pdfn_…` in every request. If you call the REST API directly, use the same header:

```bash
curl -H "X-Api-Key: pdfn_your_key_here" https://api.pdfcanon.com/api/submissions
```

## Quickstart

```csharp
using PDFCanon;

// API key can also be set via the PDFCANON_API_KEY environment variable
using var client = new PDFCanonClient("pdfn_your_api_key_here");

// Normalize a PDF file
var response = await client.NormalizeAsync("/path/to/document.pdf");
Console.WriteLine($"Status: {response.Status}");
Console.WriteLine($"Submission ID: {response.SubmissionId}");

// Download the normalized PDF
if (response.Normalized?.Sha256 is { } sha256)
{
    byte[] normalizedPdf = await client.GetArtifactAsync(sha256);
    await File.WriteAllBytesAsync("/path/to/output.pdf", normalizedPdf);
}
```

## Async / Webhook Flow

```csharp
// Submit for async processing (returns immediately with queued/processing status)
var submission = await client.SubmitAsync("/path/to/document.pdf", new NormalizeOptions
{
    WebhookUrl = "https://your-app.example.com/webhooks/pdfcanon",
    RemoveAnnotations = true,
});

// Or poll until done
var result = await client.WaitForCompletionAsync(
    Guid.Parse(submission.SubmissionId!),
    timeout: TimeSpan.FromMinutes(5));
```

## Webhook Signature Verification

```csharp
using PDFCanon.Webhooks;

// In your webhook handler:
string rawBody = await new StreamReader(Request.Body).ReadToEndAsync();
string signature = Request.Headers["X-PDFCanon-Signature"];

// Returns true/false
bool isValid = PDFCanonWebhook.VerifySignature(rawBody, signature, "your-signing-secret");

// Or throws InvalidSignatureException
PDFCanonWebhook.ValidateSignature(rawBody, signature, "your-signing-secret");
```

## Configuration

```csharp
using var client = new PDFCanonClient(new PDFCanonClientOptions
{
    ApiKey = "pdfn_your_key",
    BaseAddress = new Uri("https://api.pdfcanon.com/api/"),
    ConnectTimeout = TimeSpan.FromSeconds(5),
    ReadTimeout = TimeSpan.FromSeconds(120),
    MaxRetries = 3,
});
```

## Exception Hierarchy

| Exception | When |
|-----------|------|
| `AuthenticationException` | 401 — invalid or missing API key |
| `PolicyRejectionException` | 422 — PDF rejected by policy |
| `RateLimitException` | 429 — rate limit exceeded |
| `ToolchainException` | 5xx — server-side processing error |
| `NetworkException` | Network / timeout error |
| `PDFCanonException` | Base class for all above |

## Requirements

- .NET 6.0 or .NET 8.0
- No external NuGet packages required
