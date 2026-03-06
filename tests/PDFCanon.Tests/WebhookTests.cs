using System.Security.Cryptography;
using System.Text;
using PDFCanon.Exceptions;
using PDFCanon.Webhooks;
using Xunit;

namespace PDFCanon.Tests;

public class WebhookTests
{
    private const string Secret = "test-webhook-secret";

    private static string ComputeSignature(string body, string secret)
    {
        byte[] key = Encoding.UTF8.GetBytes(secret);
        byte[] data = Encoding.UTF8.GetBytes(body);
        byte[] hash = HMACSHA256.HashData(key, data);
        return "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    [Fact]
    public void VerifySignature_ValidSignature_ReturnsTrue()
    {
        const string body = """{"status":"success","submissionId":"abc-123"}""";
        string header = ComputeSignature(body, Secret);

        bool result = PDFCanonWebhook.VerifySignature(body, header, Secret);

        Assert.True(result);
    }

    [Fact]
    public void VerifySignature_InvalidSignature_ReturnsFalse()
    {
        const string body = """{"status":"success","submissionId":"abc-123"}""";
        const string header = "sha256=0000000000000000000000000000000000000000000000000000000000000000";

        bool result = PDFCanonWebhook.VerifySignature(body, header, Secret);

        Assert.False(result);
    }

    [Fact]
    public void VerifySignature_WrongSecret_ReturnsFalse()
    {
        const string body = """{"status":"success","submissionId":"abc-123"}""";
        string header = ComputeSignature(body, "correct-secret");

        bool result = PDFCanonWebhook.VerifySignature(body, header, "wrong-secret");

        Assert.False(result);
    }

    [Fact]
    public void VerifySignature_MissingPrefix_ReturnsFalse()
    {
        const string body = "payload";
        byte[] key = Encoding.UTF8.GetBytes(Secret);
        byte[] hash = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(body));
        string header = Convert.ToHexString(hash).ToLowerInvariant(); // no "sha256=" prefix

        bool result = PDFCanonWebhook.VerifySignature(body, header, Secret);

        Assert.False(result);
    }

    [Fact]
    public void VerifySignature_ByteOverload_ValidSignature_ReturnsTrue()
    {
        byte[] body = Encoding.UTF8.GetBytes("""{"event":"normalized"}""");
        byte[] key = Encoding.UTF8.GetBytes(Secret);
        byte[] hash = HMACSHA256.HashData(key, body);
        string header = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

        bool result = PDFCanonWebhook.VerifySignature(body, header, Secret);

        Assert.True(result);
    }

    [Fact]
    public void ValidateSignature_InvalidSignature_ThrowsInvalidSignatureException()
    {
        const string body = "payload";
        const string header = "sha256=badhash";

        Assert.Throws<InvalidSignatureException>(
            () => PDFCanonWebhook.ValidateSignature(body, header, Secret));
    }

    [Fact]
    public void ValidateSignature_ValidSignature_DoesNotThrow()
    {
        const string body = "payload";
        string header = ComputeSignature(body, Secret);

        PDFCanonWebhook.ValidateSignature(body, header, Secret); // should not throw
    }
}
