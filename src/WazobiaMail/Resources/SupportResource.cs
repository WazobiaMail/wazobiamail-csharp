using System.Globalization;
using System.Net;
using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Customer support ticket creation, listing, replies, read tracking, and attachment downloads.
/// </summary>
public sealed class SupportResource
{
    private readonly WazobiaHttpTransport _transport;

    internal SupportResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Opens a new customer support ticket.
    /// When <see cref="CreateSupportTicketRequest.Name"/> or <see cref="CreateSupportTicketRequest.Email"/> is omitted,
    /// the SDK queries <c>GET /auth/me</c> to populate them from the authenticated account.
    /// </summary>
    public async Task<ApiResponse<CreatedSupportTicket>> CreateAsync(
        CreateSupportTicketRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = request.Name;
        var email = request.Email;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            var me = await _transport.SendJsonAsync<CurrentAccount>(
                HttpMethod.Get,
                "/auth/me",
                options: options,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(name))
            {
                name = !string.IsNullOrWhiteSpace(me.Data?.Name)
                    ? me.Data.Name
                    : !string.IsNullOrWhiteSpace(me.Data?.Username)
                        ? me.Data.Username
                        : me.Data?.Email;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                email = me.Data?.Email;
            }
        }

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            var fieldErrors = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(name))
            {
                fieldErrors["name"] = "Name is required";
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                fieldErrors["email"] = "Email is required";
            }

            throw new WazobiaValidationException(
                "A support ticket requires a name and email. Pass them explicitly when using an unauthenticated client.",
                HttpStatusCode.UnprocessableEntity,
                fieldErrors: fieldErrors);
        }

        var payload = new
        {
            name,
            email,
            subject = request.Subject,
            domain = string.IsNullOrWhiteSpace(request.Domain) ? null : request.Domain,
            category = string.IsNullOrWhiteSpace(request.Category) ? "general" : request.Category,
            priority = string.IsNullOrWhiteSpace(request.Priority) ? "normal" : request.Priority,
            message = request.Message,
            contentType = string.IsNullOrWhiteSpace(request.ContentType) ? "text" : request.ContentType,
            attachments = request.Attachments
        };

        return await _transport.SendJsonAsync<CreatedSupportTicket>(
            HttpMethod.Post,
            "/support",
            payload,
            options: options,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists support tickets belonging to the authenticated account.
    /// </summary>
    public Task<ApiResponse<SupportTicketList>> ListAsync(
        SupportListParams? parameters = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(parameters?.Status))
        {
            query["status"] = parameters.Status;
        }

        if (parameters?.Page is > 0)
        {
            query["page"] = parameters.Page.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (parameters?.Limit is > 0)
        {
            query["limit"] = parameters.Limit.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrWhiteSpace(parameters?.Sort))
        {
            query["sort"] = parameters.Sort;
        }

        if (parameters?.Descending == true)
        {
            query["descending"] = "true";
        }

        return _transport.SendJsonAsync<SupportTicketList>(
            HttpMethod.Get,
            "/support/my",
            query: query,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves a single support ticket and its full message thread.
    /// </summary>
    public Task<ApiResponse<SupportTicket>> GetAsync(
        string ticketId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ticketId);
        return _transport.SendJsonAsync<SupportTicket>(
            HttpMethod.Get,
            $"/support/tickets/{Uri.EscapeDataString(ticketId)}",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Posts a reply message to a support ticket.
    /// </summary>
    public Task<ApiResponse<SupportMessage>> ReplyAsync(
        string ticketId,
        ReplySupportTicketRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ticketId);
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<SupportMessage>(
            HttpMethod.Post,
            $"/support/tickets/{Uri.EscapeDataString(ticketId)}/messages",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Marks a support ticket thread as read by the customer.
    /// </summary>
    public Task<ApiResponse<System.Text.Json.JsonElement>> MarkReadAsync(
        string ticketId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ticketId);
        return _transport.SendJsonAsync<System.Text.Json.JsonElement>(
            HttpMethod.Post,
            $"/support/tickets/{Uri.EscapeDataString(ticketId)}/read",
            new { },
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Downloads a support ticket attachment by attachment ID.
    /// </summary>
    public Task<BinaryDownload> DownloadAttachmentAsync(
        string attachmentId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attachmentId);
        return _transport.GetBytesAsync(
            $"/support/attachments/{Uri.EscapeDataString(attachmentId)}",
            options: options,
            cancellationToken: cancellationToken);
    }
}
