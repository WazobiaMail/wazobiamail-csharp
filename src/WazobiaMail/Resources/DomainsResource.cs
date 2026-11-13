using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Sending domain management, DNS record inspection, verification, and Cloudflare DNS integration.
/// </summary>
public sealed class DomainsResource
{
    private readonly WazobiaHttpTransport _transport;

    internal DomainsResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Lists all sending domains configured on the account.
    /// </summary>
    public Task<ApiResponse<ItemsEnvelope<Domain>>> ListAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<ItemsEnvelope<Domain>>(
            HttpMethod.Get,
            "/sending/domains",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Registers a new sending domain.
    /// </summary>
    public Task<ApiResponse<DomainEnvelope>> CreateAsync(
        CreateDomainRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<DomainEnvelope>(
            HttpMethod.Post,
            "/sending/domains",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves details for a single sending domain.
    /// </summary>
    public Task<ApiResponse<DomainEnvelope>> GetAsync(
        string domainId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainId);
        return _transport.SendJsonAsync<DomainEnvelope>(
            HttpMethod.Get,
            $"/sending/domains/{Uri.EscapeDataString(domainId)}",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates configuration on an existing sending domain.
    /// </summary>
    public Task<ApiResponse<DomainEnvelope>> UpdateAsync(
        string domainId,
        UpdateDomainRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainId);
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<DomainEnvelope>(
            HttpMethod.Patch,
            $"/sending/domains/{Uri.EscapeDataString(domainId)}",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes a sending domain.
    /// </summary>
    public Task<ApiResponse<DomainDeleted>> DeleteAsync(
        string domainId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainId);
        return _transport.SendJsonAsync<DomainDeleted>(
            HttpMethod.Delete,
            $"/sending/domains/{Uri.EscapeDataString(domainId)}",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Lists required and active DNS records for a sending domain.
    /// </summary>
    public Task<ApiResponse<DomainDnsEnvelope>> GetDnsRecordsAsync(
        string domainId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainId);
        return _transport.SendJsonAsync<DomainDnsEnvelope>(
            HttpMethod.Get,
            $"/sending/domains/{Uri.EscapeDataString(domainId)}/dns-records",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Triggers live DNS verification checks (DKIM, SPF, MX, tracking) for a domain.
    /// </summary>
    public Task<ApiResponse<VerificationEnvelope>> VerifyAsync(
        string domainId,
        bool force = false,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainId);
        return _transport.SendJsonAsync<VerificationEnvelope>(
            HttpMethod.Post,
            $"/sending/domains/{Uri.EscapeDataString(domainId)}/verify",
            new { force },
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Refreshes cached DNS records and re-evaluates domain verification status.
    /// </summary>
    public Task<ApiResponse<VerificationEnvelope>> RefreshDnsAsync(
        string domainId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainId);
        return _transport.SendJsonAsync<VerificationEnvelope>(
            HttpMethod.Post,
            $"/sending/domains/{Uri.EscapeDataString(domainId)}/dns-refresh",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Reads the account-level Cloudflare DNS connection status (optionally scoped to a domain).
    /// </summary>
    public Task<ApiResponse<CloudflareStatus>> GetCloudflareStatusAsync(
        string? domainId = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildOptionalDomainIdQuery(domainId);
        return _transport.SendJsonAsync<CloudflareStatus>(
            HttpMethod.Get,
            "/sending/domains/cloudflare/status",
            query: query,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Starts the Cloudflare OAuth connection flow and returns the authorization URL.
    /// </summary>
    public Task<ApiResponse<CloudflareConnect>> ConnectCloudflareAsync(
        string? domainId = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildOptionalDomainIdQuery(domainId);
        return _transport.SendJsonAsync<CloudflareConnect>(
            HttpMethod.Get,
            "/sending/domains/cloudflare/connect",
            query: query,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Publishes required DNS records for a domain to the connected Cloudflare zone.
    /// </summary>
    public Task<ApiResponse<CloudflareSyncResult>> SyncCloudflareAsync(
        string domainId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainId);
        return _transport.SendJsonAsync<CloudflareSyncResult>(
            HttpMethod.Post,
            "/sending/domains/cloudflare/sync",
            new CloudflareSyncRequest { DomainId = domainId },
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Disconnects the Cloudflare DNS integration from the account.
    /// </summary>
    public Task<ApiResponse<System.Text.Json.JsonElement>> DisconnectCloudflareAsync(
        string? domainId = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildOptionalDomainIdQuery(domainId);
        return _transport.SendJsonAsync<System.Text.Json.JsonElement>(
            HttpMethod.Delete,
            "/sending/domains/cloudflare/disconnect",
            query: query,
            options: options,
            cancellationToken: cancellationToken);
    }

    private static Dictionary<string, string>? BuildOptionalDomainIdQuery(string? domainId)
    {
        return string.IsNullOrWhiteSpace(domainId)
            ? null
            : new Dictionary<string, string> { ["domainId"] = domainId };
    }
}
