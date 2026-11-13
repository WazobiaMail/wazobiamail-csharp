using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Scoped sending API key management.
/// </summary>
public sealed class ApiKeysResource
{
    private readonly WazobiaHttpTransport _transport;

    internal ApiKeysResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Lists all sending API keys on the account.
    /// </summary>
    public Task<ApiResponse<ItemsEnvelope<ApiKey>>> ListAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<ItemsEnvelope<ApiKey>>(
            HttpMethod.Get,
            "/sending/api-keys",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates a new scoped sending API key.
    /// </summary>
    public Task<ApiResponse<ApiKeyEnvelope>> CreateAsync(
        CreateApiKeyRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<ApiKeyEnvelope>(
            HttpMethod.Post,
            "/sending/api-keys",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Regenerates an existing API key secret, invalidating the previous secret.
    /// </summary>
    public Task<ApiResponse<ApiKeyEnvelope>> RegenerateAsync(
        string keyId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId);
        return _transport.SendJsonAsync<ApiKeyEnvelope>(
            HttpMethod.Post,
            $"/sending/api-keys/{Uri.EscapeDataString(keyId)}/regenerate",
            new { },
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Revokes an API key immediately.
    /// </summary>
    public Task<ApiResponse<Dictionary<string, string>>> RevokeAsync(
        string keyId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId);
        return _transport.SendJsonAsync<Dictionary<string, string>>(
            HttpMethod.Post,
            $"/sending/api-keys/{Uri.EscapeDataString(keyId)}/revoke",
            new { },
            options: options,
            cancellationToken: cancellationToken);
    }
}
