using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Customer invoices, active subscriptions, and localized billing currency detection.
/// </summary>
public sealed class BillingResource
{
    private readonly WazobiaHttpTransport _transport;

    internal BillingResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Lists customer billing invoices.
    /// </summary>
    public Task<ApiResponse<PagedResult<Invoice>>> ListInvoicesAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<PagedResult<Invoice>>(
            HttpMethod.Get,
            "/billing/invoices",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves a single billing invoice by ID.
    /// </summary>
    public Task<ApiResponse<InvoiceEnvelope>> GetInvoiceAsync(
        string invoiceId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);
        return _transport.SendJsonAsync<InvoiceEnvelope>(
            HttpMethod.Get,
            $"/billing/invoices/{Uri.EscapeDataString(invoiceId)}",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Downloads a generated invoice PDF.
    /// </summary>
    public Task<BinaryDownload> DownloadInvoiceAsync(
        string invoiceId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);
        return _transport.GetBytesAsync(
            $"/billing/invoices/{Uri.EscapeDataString(invoiceId)}/download",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves the current active subscription for the account.
    /// </summary>
    public Task<ApiResponse<SubscriptionEnvelope>> GetCurrentSubscriptionAsync(
        string? product = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(product)
            ? null
            : new Dictionary<string, string> { ["product"] = product };

        return _transport.SendJsonAsync<SubscriptionEnvelope>(
            HttpMethod.Get,
            "/subscriptions/current",
            query: query,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Detects the localized billing currency (<c>NGN</c> or <c>USD</c>).
    /// </summary>
    public Task<ApiResponse<DetectedCurrency>> DetectCurrencyAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<DetectedCurrency>(
            HttpMethod.Get,
            "/payments/currency",
            options: options,
            cancellationToken: cancellationToken);
    }
}
