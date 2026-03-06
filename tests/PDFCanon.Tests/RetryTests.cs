using System.Net;
using System.Text;
using System.Text.Json;
using PDFCanon.Exceptions;
using Xunit;

namespace PDFCanon.Tests;

/// <summary>Fake handler that returns canned responses in sequence.</summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public int CallCount { get; private set; }

    public void Enqueue(HttpResponseMessage response)
        => _responses.Enqueue(_ => response);

    public void Enqueue(Func<HttpRequestMessage, HttpResponseMessage> factory)
        => _responses.Enqueue(factory);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CallCount++;
        if (_responses.Count == 0)
            throw new InvalidOperationException("No more responses queued.");
        return Task.FromResult(_responses.Dequeue()(request));
    }
}

public class RetryTests
{
    private static string NormalizeResponseJson(string status = "success") => JsonSerializer.Serialize(new
    {
        apiVersion = "1",
        requestId = "req-1",
        submissionId = Guid.NewGuid().ToString(),
        processingTimeMs = 100,
        status,
        original = new { sha256 = "abc", sizeBytes = 1024 },
        normalized = new { sha256 = "def", sizeBytes = 900, pdfVersion = "1.7", linearized = false, downloadUrl = "" },
        security = new { javascriptRemoved = true, openActionsRemoved = true, embeddedFilesRemoved = true, richMediaRemoved = true, launchActionsRemoved = true, incrementalUpdatesRemoved = true, acroformFlattened = true, annotationsRemoved = false, encryptedInput = false },
        validation = new { xrefRebuilt = false, objectStreamsRegenerated = false, brokenReferencesDetected = false, nonEmbeddedFontsDetected = false, pdfaCompliant = false },
        warnings = Array.Empty<object>(),
        failure = (object?)null,
    });

    private static PDFCanonClient CreateClient(FakeHttpMessageHandler handler, int maxRetries = 3)
    {
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://fake.local/api/") };
        var options = new PDFCanonClientOptions
        {
            ApiKey = "pdfn_test",
            HttpClient = http,
            MaxRetries = maxRetries,
        };
        return new PDFCanonClient(options);
    }

    [Fact]
    public async Task GetSubmission_SucceedsOnFirstAttempt_NeverRetries()
    {
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(NormalizeResponseJson(), Encoding.UTF8, "application/json"),
        });

        using var client = CreateClient(handler);
        var result = await client.GetSubmissionAsync(Guid.NewGuid());

        Assert.Equal(1, handler.CallCount);
        Assert.Equal("success", result.Status);
    }

    [Fact]
    public async Task GetSubmission_5xxThenSuccess_RetriesAndSucceeds()
    {
        var handler = new FakeHttpMessageHandler();
        // First two attempts: 500
        handler.Enqueue(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("error", Encoding.UTF8, "text/plain"),
        });
        handler.Enqueue(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("error", Encoding.UTF8, "text/plain"),
        });
        // Third attempt: 200
        handler.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(NormalizeResponseJson(), Encoding.UTF8, "application/json"),
        });

        using var client = CreateClient(handler, maxRetries: 3);
        var result = await client.GetSubmissionAsync(Guid.NewGuid());

        Assert.Equal(3, handler.CallCount);
        Assert.Equal("success", result.Status);
    }

    [Fact]
    public async Task GetSubmission_Exhausts5xxRetries_ThrowsToolchainException()
    {
        var handler = new FakeHttpMessageHandler();
        // 4 attempts (1 initial + 3 retries), all 500
        for (int i = 0; i < 4; i++)
        {
            handler.Enqueue(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("error", Encoding.UTF8, "text/plain"),
            });
        }

        using var client = CreateClient(handler, maxRetries: 3);

        await Assert.ThrowsAsync<ToolchainException>(
            () => client.GetSubmissionAsync(Guid.NewGuid()));

        Assert.Equal(4, handler.CallCount);
    }

    [Fact]
    public async Task GetSubmission_401_ThrowsAuthenticationException_NoRetry()
    {
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("unauthorized", Encoding.UTF8, "text/plain"),
        });

        using var client = CreateClient(handler);

        await Assert.ThrowsAsync<AuthenticationException>(
            () => client.GetSubmissionAsync(Guid.NewGuid()));

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetSubmission_429WithRetryAfter_ThrowsRateLimitException()
    {
        var handler = new FakeHttpMessageHandler();
        // All attempts return 429; we only want to test the exception type on exhaustion.
        for (int i = 0; i < 4; i++)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("rate limited", Encoding.UTF8, "text/plain"),
            };
            resp.Headers.Add("Retry-After", "1");
            handler.Enqueue(resp);
        }

        using var client = CreateClient(handler, maxRetries: 3);

        await Assert.ThrowsAsync<RateLimitException>(
            () => client.GetSubmissionAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetSubmission_422_ThrowsPolicyRejectionException_NoRetry()
    {
        var body = JsonSerializer.Serialize(new
        {
            failure = new { code = "DISALLOWED_CONTENT", message = "Content not allowed.", stage = "scan" },
        });
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        });

        using var client = CreateClient(handler);

        var ex = await Assert.ThrowsAsync<PolicyRejectionException>(
            () => client.GetSubmissionAsync(Guid.NewGuid()));

        Assert.Equal("DISALLOWED_CONTENT", ex.Code);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public void Constructor_NoApiKey_ThrowsArgumentException()
    {
        Environment.SetEnvironmentVariable("PDFCANON_API_KEY", null);
        var options = new PDFCanonClientOptions { ApiKey = null };

        Assert.Throws<ArgumentException>(() => new PDFCanonClient(options));
    }

    [Fact]
    public async Task Constructor_ApiKeyFromEnvironmentVariable_Succeeds()
    {
        Environment.SetEnvironmentVariable("PDFCANON_API_KEY", "pdfn_env_key");
        try
        {
            var handler = new FakeHttpMessageHandler();
            handler.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(NormalizeResponseJson(), Encoding.UTF8, "application/json"),
            });
            var http = new HttpClient(handler) { BaseAddress = new Uri("https://fake.local/api/") };
            var options = new PDFCanonClientOptions { ApiKey = null, HttpClient = http };
            using var client = new PDFCanonClient(options);

            var result = await client.GetSubmissionAsync(Guid.NewGuid());
            Assert.Equal("success", result.Status);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PDFCANON_API_KEY", null);
        }
    }
}
