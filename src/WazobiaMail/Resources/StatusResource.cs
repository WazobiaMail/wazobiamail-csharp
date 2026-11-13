using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Account plan and credit status checks.
/// </summary>
public sealed class StatusResource
{
    private readonly WazobiaHttpTransport _transport;

    internal StatusResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Retrieves the current account plan and unit balance status.
    /// </summary>
    public Task<ApiResponse<AccountStatus>> GetAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<AccountStatus>(
            HttpMethod.Get,
            "/status",
            options: options,
            cancellationToken: cancellationToken);
    }
}
