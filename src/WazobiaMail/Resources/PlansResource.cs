using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Subscription plan catalog and effective account limits.
/// </summary>
public sealed class PlansResource
{
    private readonly WazobiaHttpTransport _transport;

    internal PlansResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Lists all available subscription plans.
    /// </summary>
    public Task<ApiResponse<PagedResult<Plan>>> ListAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<PagedResult<Plan>>(
            HttpMethod.Get,
            "/plans",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves the resolved effective limits for the authenticated account.
    /// </summary>
    public Task<ApiResponse<LimitsEnvelope>> GetEffectiveLimitsAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<LimitsEnvelope>(
            HttpMethod.Get,
            "/plans/effective",
            options: options,
            cancellationToken: cancellationToken);
    }
}
