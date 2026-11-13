using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Offline HMAC-SHA256 webhook signature verification and event decoding helpers.
/// Performs no network requests.
/// </summary>
public sealed class WebhooksHelper
{
    /// <summary>
    /// The HTTP request header carrying the webhook HMAC-SHA256 signature.
    /// </summary>
    public const string SignatureHeader = "X-Wazobia-Signature";

    /// <summary>
    /// The conventional environment variable storing the webhook signing secret.
    /// </summary>
    public const string SecretEnvironmentVariable = "WAZOBIA_WEBHOOK_SECRET";

    internal WebhooksHelper()
    {
    }

    /// <summary>
    /// Computes the lowercase hex HMAC-SHA256 signature for a string payload.
    /// </summary>
    public string Sign(string payload, string secret)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return Sign(Encoding.UTF8.GetBytes(payload), secret);
    }

    /// <summary>
    /// Computes the lowercase hex HMAC-SHA256 signature for raw payload bytes.
    /// </summary>
    public string Sign(ReadOnlySpan<byte> payload, string secret)
    {
        if (string.IsNullOrEmpty(secret))
        {
            throw new WazobiaWebhookException("A webhook signing secret is required.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        Span<byte> hash = stackalloc byte[32];
        HMACSHA256.HashData(keyBytes, payload, hash);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Verifies an incoming webhook signature against the raw request body string using constant-time comparison.
    /// An optional <c>sha256=</c> prefix is accepted.
    /// </summary>
    public bool VerifySignature(string payload, string? signature, string? secret)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return VerifySignature(Encoding.UTF8.GetBytes(payload), signature, secret);
    }

    /// <summary>
    /// Verifies an incoming webhook signature against raw request body bytes using constant-time comparison.
    /// An optional <c>sha256=</c> prefix is accepted.
    /// </summary>
    public bool VerifySignature(ReadOnlySpan<byte> payload, string? signature, string? secret)
    {
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        var normalized = signature.Trim();
        if (normalized.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[7..].Trim();
        }

        if (normalized.Length != 64)
        {
            return false;
        }

        byte[] providedDigest;
        try
        {
            providedDigest = Convert.FromHexString(normalized);
        }
        catch (FormatException)
        {
            return false;
        }

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        Span<byte> expectedDigest = stackalloc byte[32];
        HMACSHA256.HashData(keyBytes, payload, expectedDigest);
        return CryptographicOperations.FixedTimeEquals(expectedDigest, providedDigest);
    }

    /// <summary>
    /// Verifies the webhook signature and parses the JSON payload into a <see cref="JsonElement"/> object.
    /// </summary>
    public JsonElement ConstructEvent(string payload, string? signature, string? secret)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return ConstructEvent(Encoding.UTF8.GetBytes(payload), signature, secret);
    }

    /// <summary>
    /// Verifies the webhook signature and parses raw JSON bytes into a <see cref="JsonElement"/> object.
    /// </summary>
    public JsonElement ConstructEvent(ReadOnlySpan<byte> payload, string? signature, string? secret)
    {
        if (!VerifySignature(payload, signature, secret))
        {
            throw new WazobiaWebhookException("Invalid webhook signature.");
        }

        try
        {
            using var doc = JsonDocument.Parse(payload.ToArray());
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new WazobiaWebhookException("Webhook payload must be a JSON object.");
            }

            return doc.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            throw new WazobiaWebhookException("Invalid JSON webhook payload.", ex);
        }
    }

    /// <summary>
    /// Verifies the webhook signature and deserializes the JSON payload into <typeparamref name="T"/>.
    /// </summary>
    public T ConstructEvent<T>(string payload, string? signature, string? secret)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return ConstructEvent<T>(Encoding.UTF8.GetBytes(payload), signature, secret);
    }

    /// <summary>
    /// Verifies the webhook signature and deserializes raw JSON bytes into <typeparamref name="T"/>.
    /// </summary>
    public T ConstructEvent<T>(ReadOnlySpan<byte> payload, string? signature, string? secret)
    {
        if (!VerifySignature(payload, signature, secret))
        {
            throw new WazobiaWebhookException("Invalid webhook signature.");
        }

        try
        {
            var result = JsonSerializer.Deserialize<T>(payload, WazobiaHttpTransport.JsonOptions);
            if (result is null)
            {
                throw new WazobiaWebhookException("Webhook payload deserialized to null.");
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new WazobiaWebhookException("Invalid JSON webhook payload.", ex);
        }
    }
}
