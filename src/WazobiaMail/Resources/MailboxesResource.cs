using System.Globalization;
using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Hosted email mailboxes, credentials, storage quotas, and headless enterprise inbox operations.
/// </summary>
public sealed class MailboxesResource
{
    private readonly WazobiaHttpTransport _transport;

    internal MailboxesResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Lists all hosted mailboxes on the account.
    /// </summary>
    public Task<ApiResponse<IReadOnlyList<Mailbox>>> ListAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<IReadOnlyList<Mailbox>>(
            HttpMethod.Get,
            "/mailboxes",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Provisions a new hosted mailbox and returns the mailbox and initial credential.
    /// </summary>
    public Task<ApiResponse<CreatedMailbox>> CreateAsync(
        CreateMailboxRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<CreatedMailbox>(
            HttpMethod.Post,
            "/mailboxes",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves details for a single mailbox.
    /// </summary>
    public Task<ApiResponse<MailboxEnvelope>> GetAsync(
        string mailboxId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailboxId);
        return _transport.SendJsonAsync<MailboxEnvelope>(
            HttpMethod.Get,
            $"/mailboxes/{Uri.EscapeDataString(mailboxId)}",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates display name or password for a mailbox.
    /// </summary>
    public Task<ApiResponse<MailboxEnvelope>> UpdateAsync(
        string mailboxId,
        UpdateMailboxRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailboxId);
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<MailboxEnvelope>(
            HttpMethod.Patch,
            $"/mailboxes/{Uri.EscapeDataString(mailboxId)}",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes a hosted mailbox.
    /// </summary>
    public Task<ApiResponse<MailboxDeleted>> DeleteAsync(
        string mailboxId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailboxId);
        return _transport.SendJsonAsync<MailboxDeleted>(
            HttpMethod.Delete,
            $"/mailboxes/{Uri.EscapeDataString(mailboxId)}",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves SMTP, IMAP, and POP3 connection settings for a mailbox.
    /// </summary>
    public Task<ApiResponse<MailboxConnection>> GetConnectionAsync(
        string mailboxId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailboxId);
        return _transport.SendJsonAsync<MailboxConnection>(
            HttpMethod.Get,
            $"/mailboxes/{Uri.EscapeDataString(mailboxId)}/connection",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves account-wide mailbox storage usage and quota allocation metrics.
    /// </summary>
    public Task<ApiResponse<MailboxStorage>> GetStorageAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<MailboxStorage>(
            HttpMethod.Get,
            "/mailboxes/storage",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates an additional SMTP/IMAP credential for a mailbox.
    /// </summary>
    public Task<ApiResponse<CredentialEnvelope>> CreateCredentialAsync(
        string mailboxId,
        CreateMailboxCredentialRequest? request = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailboxId);
        return _transport.SendJsonAsync<CredentialEnvelope>(
            HttpMethod.Post,
            $"/mailboxes/{Uri.EscapeDataString(mailboxId)}/credentials",
            request ?? new CreateMailboxCredentialRequest(),
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Rotates the active mailbox credential and returns a new password.
    /// </summary>
    public Task<ApiResponse<CredentialEnvelope>> RotateCredentialAsync(
        string mailboxId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailboxId);
        return _transport.SendJsonAsync<CredentialEnvelope>(
            HttpMethod.Post,
            $"/mailboxes/{Uri.EscapeDataString(mailboxId)}/credentials/rotate",
            new { },
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Revokes a specific mailbox credential.
    /// </summary>
    public Task<ApiResponse<MailboxCredentialRevoked>> RevokeCredentialAsync(
        string mailboxId,
        string credentialId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailboxId);
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialId);
        return _transport.SendJsonAsync<MailboxCredentialRevoked>(
            HttpMethod.Post,
            $"/mailboxes/{Uri.EscapeDataString(mailboxId)}/credentials/{Uri.EscapeDataString(credentialId)}/revoke",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Lists received messages inside a headless enterprise inbox.
    /// </summary>
    public Task<ApiResponse<MailboxMessagePage>> ListMessagesAsync(
        string mailboxId,
        ListMessagesParams? parameters = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailboxId);
        var query = new Dictionary<string, string>();
        if (parameters?.Page is > 0)
        {
            query["page"] = parameters.Page.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (parameters?.Limit is > 0)
        {
            query["limit"] = parameters.Limit.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (parameters?.UnreadOnly == true)
        {
            query["unreadOnly"] = "true";
        }

        if (!string.IsNullOrWhiteSpace(parameters?.Search))
        {
            query["search"] = parameters.Search;
        }

        return _transport.SendJsonAsync<MailboxMessagePage>(
            HttpMethod.Get,
            $"/mailboxes/{Uri.EscapeDataString(mailboxId)}/messages",
            query: query,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves a single headless inbox message including HTML/text bodies and attachments.
    /// </summary>
    public Task<ApiResponse<InboxMessageDetail>> GetMessageAsync(
        string mailboxId,
        string messageId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailboxId);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        return _transport.SendJsonAsync<InboxMessageDetail>(
            HttpMethod.Get,
            $"/mailboxes/{Uri.EscapeDataString(mailboxId)}/messages/{Uri.EscapeDataString(messageId)}",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes a message from a headless enterprise inbox.
    /// </summary>
    public Task<ApiResponse<MailboxMessageDeleted>> DeleteMessageAsync(
        string mailboxId,
        string messageId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mailboxId);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        return _transport.SendJsonAsync<MailboxMessageDeleted>(
            HttpMethod.Delete,
            $"/mailboxes/{Uri.EscapeDataString(mailboxId)}/messages/{Uri.EscapeDataString(messageId)}",
            options: options,
            cancellationToken: cancellationToken);
    }
}
