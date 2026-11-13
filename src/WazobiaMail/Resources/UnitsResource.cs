using System.Globalization;
using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Prepaid email unit balances, pack catalog, ledger history, and credit pack purchases.
/// </summary>
public sealed class UnitsResource
{
    private readonly WazobiaHttpTransport _transport;

    internal UnitsResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Retrieves the current email allowance and purchased unit balance summary.
    /// </summary>
    public Task<ApiResponse<UnitsSummary>> GetSummaryAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<UnitsSummary>(
            HttpMethod.Get,
            "/units/summary",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Lists available email unit credit packs.
    /// </summary>
    public Task<ApiResponse<ItemsEnvelope<UnitPack>>> ListPacksAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<ItemsEnvelope<UnitPack>>(
            HttpMethod.Get,
            "/units/packs",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves the paginated email unit credit ledger.
    /// </summary>
    public Task<ApiResponse<PagedResult<UnitLedgerEntry>>> GetLedgerAsync(
        int? page = null,
        int? perPage = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        if (page is > 0)
        {
            query["page"] = page.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (perPage is > 0)
        {
            query["per_page"] = perPage.Value.ToString(CultureInfo.InvariantCulture);
        }

        return _transport.SendJsonAsync<PagedResult<UnitLedgerEntry>>(
            HttpMethod.Get,
            "/units/ledger",
            query: query,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates an invoice/purchase for an email unit credit pack.
    /// </summary>
    public Task<ApiResponse<UnitPurchase>> PurchaseAsync(
        PurchaseUnitsRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<UnitPurchase>(
            HttpMethod.Post,
            "/units/purchases",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }
}
