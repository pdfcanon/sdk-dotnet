using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PDFCanon.Exceptions;
using PDFCanon.Models;

namespace PDFCanon;

/// <summary>Client for the PDFCanon normalization API.</summary>
public sealed class PDFCanonClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;
    private readonly PDFCanonClientOptions _options;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="PDFCanonClient"/> using the provided API key.
    /// </summary>
    public PDFCanonClient(string apiKey)
        : this(new PDFCanonClientOptions { ApiKey = apiKey })
    {
    }

    /// <summary>
    /// Initializes a new <see cref="PDFCanonClient"/> using the provided options.
    /// Falls back to the <c>PDFCANON_API_KEY</c> environment variable when <see cref="PDFCanonClientOptions.ApiKey"/> is null.
    /// </summary>
    public PDFCanonClient(PDFCanonClientOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        string apiKey = options.ApiKey
            ?? Environment.GetEnvironmentVariable("PDFCANON_API_KEY")
            ?? throw new ArgumentException(
                "An API key must be provided via PDFCanonClientOptions.ApiKey or the PDFCANON_API_KEY environment variable.",
                nameof(options));

        if (options.HttpClient is not null)
        {
            _http = options.HttpClient;
            _ownsHttpClient = false;
        }
        else
        {
            var handler = new SocketsHttpHandler
            {
                ConnectTimeout = options.ConnectTimeout,
            };
            _http = new HttpClient(handler)
            {
                Timeout = options.ReadTimeout,
                BaseAddress = options.BaseAddress,
            };
            _ownsHttpClient = true;
        }

        _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        _http.DefaultRequestHeaders.Add("User-Agent", "pdfcanon-dotnet/1.0.0");
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Normalizes a PDF file at the given path synchronously (waits for completion if 200, or queues if 202).</summary>
    public Task<NormalizeResponse> NormalizeAsync(
        string filePath,
        NormalizeOptions? options = null,
        CancellationToken ct = default)
    {
        if (filePath is null) throw new ArgumentNullException(nameof(filePath));
        var stream = File.OpenRead(filePath);
        return NormalizeAsyncCore(stream, Path.GetFileName(filePath), options, ct, ownsStream: true);
    }

    /// <summary>Normalizes a PDF from a stream.</summary>
    public Task<NormalizeResponse> NormalizeAsync(
        Stream fileStream,
        string fileName,
        NormalizeOptions? options = null,
        CancellationToken ct = default)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));
        if (fileName is null) throw new ArgumentNullException(nameof(fileName));
        return NormalizeAsyncCore(fileStream, fileName, options, ct, ownsStream: false);
    }

    /// <summary>Submits a PDF for normalization (always returns the queued submission immediately).</summary>
    public Task<NormalizeResponse> SubmitAsync(
        string filePath,
        NormalizeOptions? options = null,
        CancellationToken ct = default)
        => NormalizeAsync(filePath, options, ct);

    /// <summary>Gets the current status of a submission.</summary>
    public async Task<NormalizeResponse> GetSubmissionAsync(Guid submissionId, CancellationToken ct = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            using var response = await _http
                .GetAsync($"submissions/{submissionId}", ct)
                .ConfigureAwait(false);

            return await ParseResponseAsync<NormalizeResponse>(response, ct).ConfigureAwait(false);
        }, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Polls <see cref="GetSubmissionAsync"/> until the submission reaches a terminal status
    /// (success, failed, or rejected) or the <paramref name="timeout"/> elapses.
    /// </summary>
    public async Task<NormalizeResponse> WaitForCompletionAsync(
        Guid submissionId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        using var cts = timeout.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(ct)
            : null;

        if (cts is not null && timeout.HasValue)
            cts.CancelAfter(timeout.Value);

        var effectiveCt = cts?.Token ?? ct;
        var delay = TimeSpan.FromSeconds(1);

        while (true)
        {
            var submission = await GetSubmissionAsync(submissionId, effectiveCt).ConfigureAwait(false);

            if (IsTerminalStatus(submission.Status))
                return submission;

            await Task.Delay(delay, effectiveCt).ConfigureAwait(false);
        }
    }

    /// <summary>Downloads the normalized PDF bytes for the given SHA-256.</summary>
    public async Task<byte[]> GetArtifactAsync(string normalizedSha256, CancellationToken ct = default)
    {
        if (normalizedSha256 is null) throw new ArgumentNullException(nameof(normalizedSha256));
        return await ExecuteWithRetryAsync(async () =>
        {
            using var response = await _http
                .GetAsync($"artifacts/{normalizedSha256}", ct)
                .ConfigureAwait(false);

            await EnsureSuccessAsync(response, ct).ConfigureAwait(false);
            return await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        }, ct).ConfigureAwait(false);
    }

    /// <summary>Downloads the processing report bytes (JSON) for the given SHA-256.</summary>
    public async Task<byte[]> GetReportAsync(string normalizedSha256, CancellationToken ct = default)
    {
        if (normalizedSha256 is null) throw new ArgumentNullException(nameof(normalizedSha256));
        return await ExecuteWithRetryAsync(async () =>
        {
            using var response = await _http
                .GetAsync($"reports/{normalizedSha256}", ct)
                .ConfigureAwait(false);

            await EnsureSuccessAsync(response, ct).ConfigureAwait(false);
            return await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        }, ct).ConfigureAwait(false);
    }

    /// <summary>Downloads the signed attestation certificate (JWS) for the given normalized SHA-256.</summary>
    public async Task<byte[]> GetCertificateAsync(string normalizedSha256, CancellationToken ct = default)
    {
        if (normalizedSha256 is null) throw new ArgumentNullException(nameof(normalizedSha256));
        return await ExecuteWithRetryAsync(async () =>
        {
            using var response = await _http
                .GetAsync($"certificates/{normalizedSha256}", ct)
                .ConfigureAwait(false);

            await EnsureSuccessAsync(response, ct).ConfigureAwait(false);
            return await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        }, ct).ConfigureAwait(false);
    }

    /// <summary>Creates a new batch group for tracking bulk normalization workflows.</summary>
    public async Task<BatchResponse> CreateBatchAsync(string? name = null, string? webhookUrl = null, CancellationToken ct = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var payload = JsonSerializer.Serialize(new { name, webhook_url = webhookUrl });
            using var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            using var response = await _http
                .PostAsync("batches/", content, ct)
                .ConfigureAwait(false);

            return await ParseResponseAsync<BatchResponse>(response, ct).ConfigureAwait(false);
        }, ct).ConfigureAwait(false);
    }

    /// <summary>Gets the status and progress counts for a batch group.</summary>
    public async Task<BatchResponse> GetBatchAsync(Guid batchId, CancellationToken ct = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            using var response = await _http
                .GetAsync($"batches/{batchId}", ct)
                .ConfigureAwait(false);

            return await ParseResponseAsync<BatchResponse>(response, ct).ConfigureAwait(false);
        }, ct).ConfigureAwait(false);
    }

    /// <summary>Returns a paginated list of batch groups.</summary>
    public async Task<ListBatchesResponse> ListBatchesAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var query = $"batches/?page={page}&page_size={pageSize}";
            if (!string.IsNullOrEmpty(status)) query += $"&status={Uri.EscapeDataString(status)}";

            using var response = await _http
                .GetAsync(query, ct)
                .ConfigureAwait(false);

            return await ParseResponseAsync<ListBatchesResponse>(response, ct).ConfigureAwait(false);
        }, ct).ConfigureAwait(false);
    }

    /// <summary>Returns a paginated list of submissions in a batch group.</summary>
    public async Task<ListBatchSubmissionsResponse> ListBatchSubmissionsAsync(Guid batchId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            using var response = await _http
                .GetAsync($"batches/{batchId}/submissions?page={page}&page_size={pageSize}", ct)
                .ConfigureAwait(false);

            return await ParseResponseAsync<ListBatchSubmissionsResponse>(response, ct).ConfigureAwait(false);
        }, ct).ConfigureAwait(false);
    }

    /// <summary>Compares two PDFs by file path and returns a structural diff report.</summary>
    public Task<StructuralDiffReport> CompareAsync(
        string filePathA,
        string filePathB,
        bool normalizeFirst = false,
        CancellationToken ct = default)
    {
        if (filePathA is null) throw new ArgumentNullException(nameof(filePathA));
        if (filePathB is null) throw new ArgumentNullException(nameof(filePathB));
        var streamA = File.OpenRead(filePathA);
        var streamB = File.OpenRead(filePathB);
        return CompareAsyncCore(streamA, Path.GetFileName(filePathA), streamB, Path.GetFileName(filePathB), normalizeFirst, ct, ownsStreams: true);
    }

    /// <summary>Compares two PDFs from streams and returns a structural diff report.</summary>
    public Task<StructuralDiffReport> CompareAsync(
        Stream fileStreamA,
        string fileNameA,
        Stream fileStreamB,
        string fileNameB,
        bool normalizeFirst = false,
        CancellationToken ct = default)
    {
        if (fileStreamA is null) throw new ArgumentNullException(nameof(fileStreamA));
        if (fileNameA is null) throw new ArgumentNullException(nameof(fileNameA));
        if (fileStreamB is null) throw new ArgumentNullException(nameof(fileStreamB));
        if (fileNameB is null) throw new ArgumentNullException(nameof(fileNameB));
        return CompareAsyncCore(fileStreamA, fileNameA, fileStreamB, fileNameB, normalizeFirst, ct, ownsStreams: false);
    }

    // -------------------------------------------------------------------------
    // Internals
    // -------------------------------------------------------------------------

    private async Task<StructuralDiffReport> CompareAsyncCore(
        Stream streamA,
        string fileNameA,
        Stream streamB,
        string fileNameB,
        bool normalizeFirst,
        CancellationToken ct,
        bool ownsStreams)
    {
        try
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                if (ownsStreams)
                {
                    if (streamA.CanSeek) streamA.Seek(0, SeekOrigin.Begin);
                    if (streamB.CanSeek) streamB.Seek(0, SeekOrigin.Begin);
                }

                using var content = new MultipartFormDataContent();

                var fileContentA = new StreamContent(streamA);
                fileContentA.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                content.Add(fileContentA, "file_a", fileNameA);

                var fileContentB = new StreamContent(streamB);
                fileContentB.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                content.Add(fileContentB, "file_b", fileNameB);

                if (normalizeFirst)
                    content.Add(new StringContent("true"), "normalize_first");

                using var response = await _http
                    .PostAsync("compare", content, ct)
                    .ConfigureAwait(false);

                return await ParseResponseAsync<StructuralDiffReport>(response, ct).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);
        }
        finally
        {
            if (ownsStreams)
            {
                await streamA.DisposeAsync().ConfigureAwait(false);
                await streamB.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private async Task<NormalizeResponse> NormalizeAsyncCore(
        Stream stream,
        string fileName,
        NormalizeOptions? options,
        CancellationToken ct,
        bool ownsStream)
    {
        try
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                // Reset stream position for retries when the stream supports seeking.
                if (ownsStream && stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);

                using var content = BuildMultipartContent(stream, fileName, options);
                using var response = await _http
                    .PostAsync("normalize", content, ct)
                    .ConfigureAwait(false);

                return await ParseResponseAsync<NormalizeResponse>(response, ct).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);
        }
        finally
        {
            if (ownsStream)
                await stream.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static MultipartFormDataContent BuildMultipartContent(
        Stream stream,
        string fileName,
        NormalizeOptions? options)
    {
        var content = new MultipartFormDataContent();

        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", fileName);

        if (options is not null)
        {
            if (options.RemoveAnnotations)
                content.Add(new StringContent("true"), "remove_annotations");

            if (options.WebhookUrl is not null)
                content.Add(new StringContent(options.WebhookUrl), "webhook_url");

            if (options.IdempotencyKey is not null)
                content.Add(new StringContent(options.IdempotencyKey), "idempotency_key");

            if (options.SignedPdfPolicy is not null)
                content.Add(new StringContent(options.SignedPdfPolicy), "signed_pdf_policy");

            if (options.PdfaPolicy is not null)
                content.Add(new StringContent(options.PdfaPolicy), "pdfa_policy");

            if (options.Linearize.HasValue)
                content.Add(new StringContent(options.Linearize.Value ? "true" : "false"), "linearize");

            if (options.BatchId.HasValue)
                content.Add(new StringContent(options.BatchId.Value.ToString()), "batch_id");
        }

        return content;
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        int maxRetries = _options.MaxRetries;
        int attempt = 0;

        while (true)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (RateLimitException rateLimitEx) when (attempt < maxRetries)
            {
                attempt++;
                int delayMs = rateLimitEx.RetryAfterMs > 0
                    ? rateLimitEx.RetryAfterMs
                    : (int)(Math.Pow(2, attempt - 1) * 1000);
                await Task.Delay(delayMs, ct).ConfigureAwait(false);
            }
            catch (ToolchainException toolEx) when (toolEx.Retryable && attempt < maxRetries)
            {
                attempt++;
                int delayMs = (int)(Math.Pow(2, attempt - 1) * 1000);
                await Task.Delay(delayMs, ct).ConfigureAwait(false);
            }
            catch (HttpRequestException httpEx) when (attempt < maxRetries)
            {
                attempt++;
                int delayMs = (int)(Math.Pow(2, attempt - 1) * 1000);
                try
                {
                    await Task.Delay(delayMs, ct).ConfigureAwait(false);
                }
                catch
                {
                    throw new NetworkException("A network error occurred while communicating with the PDFCanon API.", httpEx);
                }
            }
            catch (HttpRequestException httpEx)
            {
                throw new NetworkException("A network error occurred while communicating with the PDFCanon API.", httpEx);
            }
            catch (TaskCanceledException tcEx) when (!ct.IsCancellationRequested)
            {
                throw new NetworkException("The request to the PDFCanon API timed out.", tcEx);
            }
        }
    }

    private static async Task<T> ParseResponseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        await EnsureSuccessAsync(response, ct).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        var result = JsonSerializer.Deserialize<T>(body, JsonOptions);

        if (result is null)
            throw new PDFCanonException("Received an empty or invalid response from the PDFCanon API.");

        return result;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        string? requestId = response.Headers.TryGetValues("X-Request-ID", out var ids)
            ? System.Linq.Enumerable.FirstOrDefault(ids)
            : null;

        string? body = null;
        try
        {
            body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }
        catch { /* best-effort */ }

        int statusCode = (int)response.StatusCode;

        if (statusCode == 401)
            throw new AuthenticationException("Authentication failed. Verify your API key.", requestId);

        if (statusCode == 422)
        {
            string code = TryExtractFailureCode(body) ?? "unknown";
            string message = TryExtractFailureMessage(body) ?? "The request was rejected by policy.";
            throw new PolicyRejectionException(message, code, requestId);
        }

        if (statusCode == 429)
        {
            int retryAfterMs = ParseRetryAfterMs(response);
            throw new RateLimitException(
                "Rate limit exceeded. Please slow down your request rate.", retryAfterMs, requestId);
        }

        if (statusCode >= 500)
            throw new ToolchainException(
                $"The PDFCanon API returned a server error ({statusCode}).",
                retryable: true,
                requestId);

        throw new PDFCanonException(
            $"Unexpected API response: {statusCode}. Body: {body}", requestId);
    }

    private static string? TryExtractFailureCode(string? body)
    {
        if (body is null) return null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("failure", out var failure) &&
                failure.TryGetProperty("code", out var code))
                return code.GetString();
        }
        catch { /* best-effort */ }
        return null;
    }

    private static string? TryExtractFailureMessage(string? body)
    {
        if (body is null) return null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("failure", out var failure) &&
                failure.TryGetProperty("message", out var msg))
                return msg.GetString();
        }
        catch { /* best-effort */ }
        return null;
    }

    private static int ParseRetryAfterMs(HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter?.Delta is { } delta)
            return (int)delta.TotalMilliseconds;

        if (response.Headers.RetryAfter?.Date is { } date)
        {
            int ms = (int)(date - DateTimeOffset.UtcNow).TotalMilliseconds;
            return ms > 0 ? ms : 0;
        }

        return 0;
    }

    private static bool IsTerminalStatus(string? status) =>
        status?.ToLowerInvariant() is "success" or "failed" or "rejected";

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_ownsHttpClient)
            _http.Dispose();
    }
}
