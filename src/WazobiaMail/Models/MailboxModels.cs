using System.Text.Json;
using System.Text.Json.Serialization;

namespace WazobiaMail;

/// <summary>
/// Hosted email mailbox entity.
/// </summary>
public sealed class Mailbox
{
    /// <summary>
    /// Gets or sets the mailbox ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owning domain ID.
    /// </summary>
    public string DomainId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full mailbox email address (for example <c>support@example.com</c>).
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the local part before <c>@</c>.
    /// </summary>
    public string LocalPart { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the mailbox status (<c>active</c> or <c>suspended</c>).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the mailbox belongs to a platform subdomain.
    /// </summary>
    public bool IsPlatformSubdomain { get; set; }

    /// <summary>
    /// Gets or sets whether the mailbox was auto-provisioned as a service address.
    /// </summary>
    public bool AutoProvisioned { get; set; }

    /// <summary>
    /// Gets or sets the mailbox role (<c>general</c> or <c>admin</c>).
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public string? UpdatedAt { get; set; }
}

/// <summary>
/// Wrapper envelope for single mailbox responses (<c>{"mailbox": {...}}</c>).
/// </summary>
public sealed class MailboxEnvelope
{
    /// <summary>
    /// Gets or sets the mailbox record.
    /// </summary>
    public Mailbox Mailbox { get; set; } = new();
}

/// <summary>
/// Response returned when provisioning a new mailbox, including its initial credential.
/// </summary>
public sealed class CreatedMailbox
{
    /// <summary>
    /// Gets or sets the provisioned mailbox.
    /// </summary>
    public Mailbox Mailbox { get; set; } = new();

    /// <summary>
    /// Gets or sets the initial credential and one-time password.
    /// </summary>
    public CreatedCredential Credential { get; set; } = new();
}

/// <summary>
/// Result returned when deleting a mailbox.
/// </summary>
public sealed class MailboxDeleted
{
    /// <summary>
    /// Gets or sets the deleted mailbox ID.
    /// </summary>
    public string MailboxId { get; set; } = string.Empty;
}

/// <summary>
/// Request payload for provisioning a hosted mailbox.
/// </summary>
public sealed class CreateMailboxRequest
{
    /// <summary>
    /// Gets or sets the verified domain ID.
    /// </summary>
    public string DomainId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the local part before <c>@</c>.
    /// </summary>
    public string LocalPart { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets an optional initial password (generated automatically if omitted).
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets an optional storage quota in megabytes.
    /// </summary>
    public int? QuotaMb { get; set; }

    /// <summary>
    /// Gets or sets the mailbox role (<c>general</c> or <c>admin</c>).
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets the initial status (<c>active</c> or <c>suspended</c>).
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// Request payload for updating a mailbox.
/// </summary>
public sealed class UpdateMailboxRequest
{
    /// <summary>
    /// Gets or sets the updated display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets a new mailbox password.
    /// </summary>
    public string? Password { get; set; }
}

/// <summary>
/// Mailbox SMTP/IMAP/POP3 authentication credential metadata.
/// </summary>
public sealed class MailboxCredential
{
    /// <summary>
    /// Gets or sets the credential ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owning mailbox ID.
    /// </summary>
    public string MailboxId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP/IMAP username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional expiration timestamp.
    /// </summary>
    public string? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the revocation timestamp if revoked.
    /// </summary>
    public string? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }
}

/// <summary>
/// Newly created mailbox credential paired with its one-time plaintext password.
/// </summary>
public sealed class CreatedCredential
{
    /// <summary>
    /// Gets or sets the credential metadata.
    /// </summary>
    public MailboxCredential Credential { get; set; } = new();

    /// <summary>
    /// Gets or sets the one-time generated password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Wrapper envelope for mailbox credential responses (<c>{"credential": {...}}</c>).
/// </summary>
public sealed class CredentialEnvelope
{
    /// <summary>
    /// Gets or sets the created credential and password.
    /// </summary>
    public CreatedCredential Credential { get; set; } = new();
}

/// <summary>
/// Request payload for creating a mailbox credential.
/// </summary>
public sealed class CreateMailboxCredentialRequest
{
    /// <summary>
    /// Gets or sets an optional ISO-8601 expiration timestamp.
    /// </summary>
    public string? ExpiresAt { get; set; }
}

/// <summary>
/// Result returned when revoking a mailbox credential.
/// </summary>
public sealed class MailboxCredentialRevoked
{
    /// <summary>
    /// Gets or sets the mailbox ID.
    /// </summary>
    public string MailboxId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the revoked credential ID.
    /// </summary>
    public string CredentialId { get; set; } = string.Empty;
}

/// <summary>
/// Mail client connection settings (SMTP/IMAP/POP3) for a mailbox.
/// </summary>
public sealed class MailboxConnection
{
    /// <summary>
    /// Gets or sets the mailbox ID.
    /// </summary>
    public string MailboxId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the domain ID.
    /// </summary>
    public string DomainId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the login username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mailbox status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets SMTP server settings.
    /// </summary>
    public JsonElement? Smtp { get; set; }

    /// <summary>
    /// Gets or sets IMAP server settings.
    /// </summary>
    public JsonElement? Imap { get; set; }

    /// <summary>
    /// Gets or sets POP3 server settings.
    /// </summary>
    public JsonElement? Pop3 { get; set; }

    /// <summary>
    /// Gets or sets the authentication scheme description.
    /// </summary>
    public string Authentication { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password hint.
    /// </summary>
    public string PasswordHint { get; set; } = string.Empty;
}

/// <summary>
/// Per-mailbox storage consumption row.
/// </summary>
public sealed class MailboxStorageUsage
{
    /// <summary>
    /// Gets or sets the mailbox ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account ID.
    /// </summary>
    public long? AccountId { get; set; }

    /// <summary>
    /// Gets or sets the mailbox email address.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets bytes currently used.
    /// </summary>
    public long? UsedBytes { get; set; }

    /// <summary>
    /// Gets or sets quota in bytes.
    /// </summary>
    public long? QuotaBytes { get; set; }

    /// <summary>
    /// Gets or sets remaining bytes.
    /// </summary>
    public long? RemainingBytes { get; set; }

    /// <summary>
    /// Gets or sets the measurement timestamp.
    /// </summary>
    public string? MeasuredAt { get; set; }
}

/// <summary>
/// Account-wide storage summary row.
/// </summary>
public sealed class MailboxStorageCustomer
{
    /// <summary>
    /// Gets or sets the account ID.
    /// </summary>
    public long? AccountId { get; set; }

    /// <summary>
    /// Gets or sets total bytes used.
    /// </summary>
    public long? UsedBytes { get; set; }

    /// <summary>
    /// Gets or sets total bytes allocated across mailboxes.
    /// </summary>
    public long AllocatedBytes { get; set; }

    /// <summary>
    /// Gets or sets total quota bytes.
    /// </summary>
    public long? QuotaBytes { get; set; }

    /// <summary>
    /// Gets or sets the mailbox count.
    /// </summary>
    public long Mailboxes { get; set; }

    /// <summary>
    /// Gets or sets the measurement timestamp.
    /// </summary>
    public string? MeasuredAt { get; set; }
}

/// <summary>
/// Per-domain storage summary row.
/// </summary>
public sealed class MailboxStorageDomain
{
    /// <summary>
    /// Gets or sets the domain ID.
    /// </summary>
    public string DomainId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mailbox count.
    /// </summary>
    public long Mailboxes { get; set; }

    /// <summary>
    /// Gets or sets included mailboxes count.
    /// </summary>
    public long IncludedMailboxes { get; set; }

    /// <summary>
    /// Gets or sets bytes used.
    /// </summary>
    public long? UsedBytes { get; set; }

    /// <summary>
    /// Gets or sets bytes allocated.
    /// </summary>
    public long AllocatedBytes { get; set; }

    /// <summary>
    /// Gets or sets the measurement timestamp.
    /// </summary>
    public string? MeasuredAt { get; set; }
}

/// <summary>
/// Plan mailbox and storage allocation block.
/// </summary>
public sealed class MailboxStoragePlan
{
    /// <summary>
    /// Gets or sets the total mailbox limit.
    /// </summary>
    public long MailboxLimit { get; set; }

    /// <summary>
    /// Gets or sets the base plan mailbox limit.
    /// </summary>
    public long BaseMailboxLimit { get; set; }

    /// <summary>
    /// Gets or sets purchased additional mailbox slots.
    /// </summary>
    public long AdditionalMailboxLimit { get; set; }

    /// <summary>
    /// Gets or sets the number of mailboxes currently provisioned.
    /// </summary>
    public long MailboxesUsed { get; set; }

    /// <summary>
    /// Gets or sets remaining mailbox slots.
    /// </summary>
    public long? MailboxRemaining { get; set; }

    /// <summary>
    /// Gets or sets total storage allowance in bytes.
    /// </summary>
    public long? StorageTotalBytes { get; set; }

    /// <summary>
    /// Gets or sets allocated storage in bytes.
    /// </summary>
    public long StorageAllocatedBytes { get; set; }

    /// <summary>
    /// Gets or sets remaining storage in bytes.
    /// </summary>
    public long? StorageRemainingBytes { get; set; }
}

/// <summary>
/// Account-wide mailbox storage and quota report.
/// </summary>
public sealed class MailboxStorage
{
    /// <summary>
    /// Gets or sets per-mailbox storage rows.
    /// </summary>
    public IReadOnlyList<MailboxStorageUsage> Mailboxes { get; set; } = Array.Empty<MailboxStorageUsage>();

    /// <summary>
    /// Gets or sets customer summary rows.
    /// </summary>
    public IReadOnlyList<MailboxStorageCustomer> Customers { get; set; } = Array.Empty<MailboxStorageCustomer>();

    /// <summary>
    /// Gets or sets per-domain summary rows.
    /// </summary>
    public IReadOnlyList<MailboxStorageDomain> Domains { get; set; } = Array.Empty<MailboxStorageDomain>();

    /// <summary>
    /// Gets or sets the measurement source.
    /// </summary>
    public string Measurement { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plan allocation details.
    /// </summary>
    public MailboxStoragePlan? Plan { get; set; }
}

/// <summary>
/// Name and email address pair in an inbox message.
/// </summary>
public sealed class EmailAddress
{
    /// <summary>
    /// Gets or sets the optional display name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Attachment metadata on a received headless inbox message.
/// </summary>
public sealed class InboxAttachment
{
    /// <summary>
    /// Gets or sets the attachment blob ID.
    /// </summary>
    public string BlobId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the attachment file name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME content type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the attachment size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }
}

/// <summary>
/// Summary of a received message in a headless inbox.
/// </summary>
public class InboxMessage
{
    /// <summary>
    /// Gets or sets the message ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the thread ID.
    /// </summary>
    public string? ThreadId { get; set; }

    /// <summary>
    /// Gets or sets the subject line.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets a short body preview snippet.
    /// </summary>
    public string? Preview { get; set; }

    /// <summary>
    /// Gets or sets the sender addresses.
    /// </summary>
    public IReadOnlyList<EmailAddress> From { get; set; } = Array.Empty<EmailAddress>();

    /// <summary>
    /// Gets or sets the To recipient addresses.
    /// </summary>
    public IReadOnlyList<EmailAddress> To { get; set; } = Array.Empty<EmailAddress>();

    /// <summary>
    /// Gets or sets the received timestamp.
    /// </summary>
    public string? ReceivedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message is unread.
    /// </summary>
    public bool IsUnread { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message has attachments.
    /// </summary>
    public bool HasAttachments { get; set; }

    /// <summary>
    /// Gets or sets the message size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }
}

/// <summary>
/// Full headless inbox message including HTML/text bodies and attachments.
/// </summary>
public sealed class InboxMessageDetail : InboxMessage
{
    /// <summary>
    /// Gets or sets the CC recipient addresses.
    /// </summary>
    public IReadOnlyList<EmailAddress> Cc { get; set; } = Array.Empty<EmailAddress>();

    /// <summary>
    /// Gets or sets the BCC recipient addresses.
    /// </summary>
    public IReadOnlyList<EmailAddress> Bcc { get; set; } = Array.Empty<EmailAddress>();

    /// <summary>
    /// Gets or sets the Reply-To addresses.
    /// </summary>
    public IReadOnlyList<EmailAddress> ReplyTo { get; set; } = Array.Empty<EmailAddress>();

    /// <summary>
    /// Gets or sets the HTML body.
    /// </summary>
    public string? HtmlBody { get; set; }

    /// <summary>
    /// Gets or sets the plain-text body.
    /// </summary>
    public string? TextBody { get; set; }

    /// <summary>
    /// Gets or sets the message attachments.
    /// </summary>
    public IReadOnlyList<InboxAttachment> Attachments { get; set; } = Array.Empty<InboxAttachment>();
}

/// <summary>
/// Paginated list of messages in a headless inbox.
/// </summary>
public sealed class MailboxMessagePage
{
    /// <summary>
    /// Gets or sets the inbox message summaries.
    /// </summary>
    public IReadOnlyList<InboxMessage> Items { get; set; } = Array.Empty<InboxMessage>();

    /// <summary>
    /// Gets or sets the total number of matching messages.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Gets or sets the current 1-based page.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the page size limit.
    /// </summary>
    public int Limit { get; set; }
}

/// <summary>
/// Query filters for listing headless inbox messages.
/// </summary>
public sealed class ListMessagesParams
{
    /// <summary>
    /// Gets or sets the 1-based page number.
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// Gets or sets the page size limit.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Gets or sets whether to filter to unread messages only.
    /// </summary>
    public bool UnreadOnly { get; set; }

    /// <summary>
    /// Gets or sets an optional search query string.
    /// </summary>
    public string? Search { get; set; }
}

/// <summary>
/// Result returned when deleting a headless inbox message.
/// </summary>
public sealed class MailboxMessageDeleted
{
    /// <summary>
    /// Gets or sets the deleted message ID.
    /// </summary>
    public string MessageId { get; set; } = string.Empty;
}
