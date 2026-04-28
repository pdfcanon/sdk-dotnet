using System;
using System.Security.Cryptography;
using System.Text;
using PDFCanon.Exceptions;

namespace PDFCanon.Webhooks;

/// <summary>Utility for verifying PDFCanon webhook signatures.</summary>
public static class PDFCanonWebhook
{
    private const string SignaturePrefix = "sha256=";

    /// <summary>
    /// Returns <c>true</c> if the <paramref name="signatureHeader"/> matches the HMAC-SHA256 of
    /// <paramref name="rawBody"/> computed with <paramref name="secret"/>.
    /// </summary>
    public static bool VerifySignature(string rawBody, string signatureHeader, string secret)
        => VerifySignature(Encoding.UTF8.GetBytes(rawBody), signatureHeader, secret);

    /// <inheritdoc cref="VerifySignature(string, string, string)"/>
    public static bool VerifySignature(byte[] rawBody, string signatureHeader, string secret)
    {
        if (rawBody is null) throw new ArgumentNullException(nameof(rawBody));
        if (signatureHeader is null) throw new ArgumentNullException(nameof(signatureHeader));
        if (secret is null) throw new ArgumentNullException(nameof(secret));

        if (!signatureHeader.StartsWith(SignaturePrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var providedHex = signatureHeader[SignaturePrefix.Length..];

        byte[] key = Encoding.UTF8.GetBytes(secret);
        byte[] computedHash = HMACSHA256.HashData(key, rawBody);
        string computedHex = Convert.ToHexString(computedHash).ToLowerInvariant();

        byte[] providedBytes = Encoding.UTF8.GetBytes(providedHex.ToLowerInvariant());
        byte[] computedBytes = Encoding.UTF8.GetBytes(computedHex);

        return CryptographicOperations.FixedTimeEquals(providedBytes, computedBytes);
    }

    /// <summary>
    /// Validates the webhook signature. Throws <see cref="InvalidSignatureException"/> if invalid.
    /// </summary>
    public static void ValidateSignature(string rawBody, string signatureHeader, string secret)
    {
        if (!VerifySignature(rawBody, signatureHeader, secret))
            throw new InvalidSignatureException("Webhook signature verification failed.");
    }

    /// <inheritdoc cref="ValidateSignature(string, string, string)"/>
    public static void ValidateSignature(byte[] rawBody, string signatureHeader, string secret)
    {
        if (!VerifySignature(rawBody, signatureHeader, secret))
            throw new InvalidSignatureException("Webhook signature verification failed.");
    }
}
