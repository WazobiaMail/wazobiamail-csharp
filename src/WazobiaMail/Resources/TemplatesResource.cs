using System.Globalization;
using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Email, SMS, and WhatsApp template management and preview rendering.
/// </summary>
public sealed class TemplatesResource
{
    private readonly WazobiaHttpTransport _transport;

    internal TemplatesResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Lists templates matching optional filters.
    /// </summary>
    public Task<ApiResponse<TemplateList>> ListAsync(
        TemplateListParams? parameters = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(parameters?.Channel))
        {
            query["channel"] = parameters.Channel;
        }

        if (!string.IsNullOrWhiteSpace(parameters?.Category))
        {
            query["category"] = parameters.Category;
        }

        if (!string.IsNullOrWhiteSpace(parameters?.Status))
        {
            query["status"] = parameters.Status;
        }

        if (!string.IsNullOrWhiteSpace(parameters?.Search))
        {
            query["search"] = parameters.Search;
        }

        if (parameters?.Limit is > 0)
        {
            query["limit"] = parameters.Limit.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (parameters?.Offset is > 0)
        {
            query["offset"] = parameters.Offset.Value.ToString(CultureInfo.InvariantCulture);
        }

        return _transport.SendJsonAsync<TemplateList>(
            HttpMethod.Get,
            "/templates",
            query: query,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates a new template.
    /// </summary>
    public Task<ApiResponse<Template>> CreateAsync(
        CreateTemplateRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<Template>(
            HttpMethod.Post,
            "/templates",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves a single template by ID.
    /// </summary>
    public Task<ApiResponse<Template>> GetAsync(
        string templateId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);
        return _transport.SendJsonAsync<Template>(
            HttpMethod.Get,
            $"/templates/{Uri.EscapeDataString(templateId)}",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates an existing template.
    /// </summary>
    public Task<ApiResponse<Template>> UpdateAsync(
        string templateId,
        UpdateTemplateRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<Template>(
            HttpMethod.Put,
            $"/templates/{Uri.EscapeDataString(templateId)}",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes a template.
    /// </summary>
    public Task<ApiResponse<Dictionary<string, string>>> DeleteAsync(
        string templateId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);
        return _transport.SendJsonAsync<Dictionary<string, string>>(
            HttpMethod.Delete,
            $"/templates/{Uri.EscapeDataString(templateId)}",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Renders a template preview with sample variable values.
    /// </summary>
    public Task<ApiResponse<TemplatePreview>> PreviewAsync(
        string templateId,
        PreviewTemplateRequest? request = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);
        return _transport.SendJsonAsync<TemplatePreview>(
            HttpMethod.Post,
            $"/templates/{Uri.EscapeDataString(templateId)}/preview",
            request ?? new PreviewTemplateRequest(),
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Synchronizes a WhatsApp template's approval status with Meta.
    /// </summary>
    public Task<ApiResponse<Template>> SyncMetaAsync(
        string templateId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);
        return _transport.SendJsonAsync<Template>(
            HttpMethod.Post,
            $"/templates/{Uri.EscapeDataString(templateId)}/sync-meta",
            new { },
            options: options,
            cancellationToken: cancellationToken);
    }
}
