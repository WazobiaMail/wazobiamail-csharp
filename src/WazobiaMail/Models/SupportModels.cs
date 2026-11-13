namespace WazobiaMail;

/// <summary>
/// Stored attachment metadata on a support ticket message.
/// </summary>
public sealed class SupportAttachment
{
    /// <summary>
    /// Gets or sets the attachment ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME content type.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the download URL if provided.
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// Inline base64-encoded attachment included when opening or replying to a support ticket.
/// </summary>
public sealed class SupportUpload
{
    /// <summary>
    /// Gets or sets the attachment file name.
    /// </summary>
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base64-encoded file contents (max 5 MiB per file).
    /// </summary>
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Single message inside a support ticket conversation thread.
/// </summary>
public sealed class SupportMessage
{
    /// <summary>
    /// Gets or sets the message ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent ticket ID.
    /// </summary>
    public string TicketId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender name.
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender email address.
    /// </summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message body.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this message was sent by WazobiaMail support staff.
    /// </summary>
    public bool IsFromStaff { get; set; }

    /// <summary>
    /// Gets or sets the content type (<c>text</c> or <c>html</c>).
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the attachments on this message.
    /// </summary>
    public IReadOnlyList<SupportAttachment> Attachments { get; set; } = Array.Empty<SupportAttachment>();

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }
}

/// <summary>
/// Customer support ticket with its message thread.
/// </summary>
public sealed class SupportTicket
{
    /// <summary>
    /// Gets or sets the ticket ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account ID.
    /// </summary>
    public long? AccountId { get; set; }

    /// <summary>
    /// Gets or sets the requester name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requester email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ticket subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional related domain name.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Gets or sets the ticket category.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ticket priority.
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ticket status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conversation messages.
    /// </summary>
    public IReadOnlyList<SupportMessage> Messages { get; set; } = Array.Empty<SupportMessage>();

    /// <summary>
    /// Gets or sets when the customer last read the ticket.
    /// </summary>
    public string? CustomerReadAt { get; set; }

    /// <summary>
    /// Gets or sets when staff last read the ticket.
    /// </summary>
    public string? StaffReadAt { get; set; }

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
/// Paginated list of customer support tickets.
/// </summary>
public sealed class SupportTicketList
{
    /// <summary>
    /// Gets or sets the ticket items.
    /// </summary>
    public IReadOnlyList<SupportTicket> Items { get; set; } = Array.Empty<SupportTicket>();

    /// <summary>
    /// Gets or sets the total matching tickets.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// Gets or sets the current 1-based page.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    public int Pages { get; set; }
}

/// <summary>
/// Request payload for creating a customer support ticket.
/// When <see cref="Name"/> or <see cref="Email"/> is omitted, the SDK attempts to fill them from <c>GET /auth/me</c>.
/// </summary>
public sealed class CreateSupportTicketRequest
{
    /// <summary>
    /// Gets or sets the requester's full name (optional when authenticated).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the requester's email address (optional when authenticated).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the ticket subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional related domain name.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Gets or sets the ticket category (defaults to <c>general</c>).
    /// </summary>
    public string Category { get; set; } = "general";

    /// <summary>
    /// Gets or sets the ticket priority (defaults to <c>normal</c>).
    /// </summary>
    public string Priority { get; set; } = "normal";

    /// <summary>
    /// Gets or sets the initial message body.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the body content type (<c>text</c> or <c>html</c>).
    /// </summary>
    public string ContentType { get; set; } = "text";

    /// <summary>
    /// Gets or sets optional inline attachments.
    /// </summary>
    public IList<SupportUpload>? Attachments { get; set; }
}

/// <summary>
/// Receipt returned when a support ticket is created.
/// </summary>
public sealed class CreatedSupportTicket
{
    /// <summary>
    /// Gets or sets the ticket ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ticket reference ID.
    /// </summary>
    public string TicketId { get; set; } = string.Empty;
}

/// <summary>
/// Request payload for replying to an existing support ticket.
/// </summary>
public sealed class ReplySupportTicketRequest
{
    /// <summary>
    /// Gets or sets the reply message body.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content type (<c>text</c> or <c>html</c>).
    /// </summary>
    public string ContentType { get; set; } = "text";

    /// <summary>
    /// Gets or sets optional inline attachments.
    /// </summary>
    public IList<SupportUpload>? Attachments { get; set; }
}

/// <summary>
/// Query filters for listing support tickets.
/// </summary>
public sealed class SupportListParams
{
    /// <summary>
    /// Gets or sets an optional ticket status filter.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the 1-based page number.
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// Gets or sets the maximum rows per page.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// Gets or sets whether to sort descending.
    /// </summary>
    public bool Descending { get; set; }
}
