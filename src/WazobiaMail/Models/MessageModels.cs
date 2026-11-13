using System.Text.Json;
using System.Text.Json.Serialization;

namespace WazobiaMail;

/// <summary>
/// Publicly reachable URL attachment included in a JSON email send request.
/// </summary>
public sealed class UrlAttachment
{
    /// <summary>
    /// Initializes a new <see cref="UrlAttachment"/> instance.
    /// </summary>
    public UrlAttachment()
    {
    }

    /// <summary>
    /// Initializes a new <see cref="UrlAttachment"/> instance with a URL and file name.
    /// </summary>
    public UrlAttachment(string url, string fileName)
    {
        Url = url;
        FileName = fileName;
    }

    /// <summary>
    /// Gets or sets the publicly accessible attachment URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the attachment file name displayed to recipients.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Binary attachment for multipart email sends and attachment uploads.
/// </summary>
public sealed class FileUpload
{
    /// <summary>
    /// Initializes a new <see cref="FileUpload"/> instance.
    /// </summary>
    public FileUpload(string fileName, byte[] content, string contentType = "application/octet-stream")
    {
        FileName = fileName;
        Content = content;
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
    }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the MIME content type.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the raw file bytes.
    /// </summary>
    public byte[] Content { get; set; }

    /// <summary>
    /// Reads a file from disk into a <see cref="FileUpload"/>.
    /// </summary>
    public static async Task<FileUpload> FromFileAsync(
        string path,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return new FileUpload(
            Path.GetFileName(path),
            bytes,
            contentType ?? "application/octet-stream");
    }
}

/// <summary>
/// Ergonomic request for sending a transactional email via WazobiaMail.
/// </summary>
public sealed class SendMessageRequest
{
    /// <summary>
    /// Gets or sets the primary recipient email addresses.
    /// </summary>
    public IList<string> To { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets optional CC recipient email addresses.
    /// </summary>
    public IList<string> Cc { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets optional BCC recipient email addresses.
    /// </summary>
    public IList<string> Bcc { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the sender address (for example <c>"Acme &lt;hello@example.com&gt;"</c>).
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Gets or sets the email subject line.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTML body.
    /// </summary>
    public string? Html { get; set; }

    /// <summary>
    /// Gets or sets the plain-text body.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the optional Reply-To email address.
    /// </summary>
    public string? ReplyTo { get; set; }

    /// <summary>
    /// Gets or sets an optional envelope sender (Return-Path) override.
    /// </summary>
    public string? EnvelopeSender { get; set; }

    /// <summary>
    /// Gets or sets optional URL attachments.
    /// </summary>
    public IList<UrlAttachment> Attachments { get; set; } = new List<UrlAttachment>();

    /// <summary>
    /// Gets or sets an optional <c>Idempotency-Key</c> header for safe retries.
    /// </summary>
    public string? IdempotencyKey { get; set; }
}

internal sealed class SendContentPayload
{
    public string? From { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public string? HtmlBody { get; set; }
    public string? ReplyTo { get; set; }
}

internal sealed class SendMessagePayload
{
    public IReadOnlyList<string> To { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string>? Cc { get; set; }
    public IReadOnlyList<string>? Bcc { get; set; }
    public SendContentPayload Content { get; set; } = new();
    public string? EnvelopeSender { get; set; }
    public IReadOnlyList<UrlAttachment>? Attachments { get; set; }
}

/// <summary>
/// Send receipt returned by the transactional send endpoints.
/// </summary>
public sealed class SendResult
{
    /// <summary>
    /// Gets or sets the delivery acceptance status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message tracking ID.
    /// </summary>
    public string TrackingId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of accepted recipients.
    /// </summary>
    public int Recipients { get; set; }
}

/// <summary>
/// Wrapper envelope for send responses (<c>{"result": {...}}</c>).
/// </summary>
public sealed class SendEnvelope
{
    /// <summary>
    /// Gets or sets the send receipt.
    /// </summary>
    public SendResult Result { get; set; } = new();
}

/// <summary>
/// Metadata for a file uploaded via <c>/sending/attachments/upload</c>.
/// </summary>
public sealed class UploadedFile
{
    /// <summary>
    /// Gets or sets the stored file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detected MIME content type.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the public CDN URL usable in <see cref="UrlAttachment"/>.
    /// </summary>
    public string? PublicUrl { get; set; }
}

/// <summary>
/// Wrapper envelope for uploaded attachment responses (<c>{"file": {...}}</c>).
/// </summary>
public sealed class UploadedFileEnvelope
{
    /// <summary>
    /// Gets or sets the uploaded file metadata.
    /// </summary>
    public UploadedFile File { get; set; } = new();
}

/// <summary>
/// Request payload for pre-flight spam and deliverability checking.
/// </summary>
public sealed class SpamCheckRequest
{
    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional HTML body.
    /// </summary>
    public string? Html { get; set; }

    /// <summary>
    /// Gets or sets the optional plain-text body.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the optional From header value.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Gets or sets the optional Reply-To address.
    /// </summary>
    public string? ReplyTo { get; set; }

    /// <summary>
    /// Gets or sets optional custom email headers to evaluate.
    /// </summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>
/// Individual rule finding returned by the spam scanner.
/// </summary>
public sealed class SpamRule
{
    /// <summary>
    /// Gets or sets the rule code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the score contribution in points.
    /// </summary>
    public double Points { get; set; }

    /// <summary>
    /// Gets or sets the short finding title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed explanation.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the remediation recommendation.
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Pre-flight spam and deliverability report.
/// </summary>
public sealed class SpamCheckResult
{
    /// <summary>
    /// Gets or sets the computed spam score.
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Gets or sets the spam threshold.
    /// </summary>
    public double Threshold { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the draft is classified as spam.
    /// </summary>
    public bool IsSpam { get; set; }

    /// <summary>
    /// Gets or sets the deliverability rating label.
    /// </summary>
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the badge color hint.
    /// </summary>
    public string BadgeColor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overall deliverability score (0–100).
    /// </summary>
    public int DeliverabilityScore { get; set; }

    /// <summary>
    /// Gets or sets the summary text.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the triggered rule findings.
    /// </summary>
    public IReadOnlyList<SpamRule> Rules { get; set; } = Array.Empty<SpamRule>();
}

/// <summary>
/// Tracked outbound message metadata.
/// </summary>
public sealed class TrackingMessage
{
    /// <summary>
    /// Gets or sets the tracking identifier.
    /// </summary>
    public string TrackingId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender domain.
    /// </summary>
    public string SenderDomain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the total recipient count.
    /// </summary>
    public int RecipientsCount { get; set; }

    /// <summary>
    /// Gets or sets the accepted count.
    /// </summary>
    public int AcceptedCount { get; set; }

    /// <summary>
    /// Gets or sets the delivered count.
    /// </summary>
    public int DeliveredCount { get; set; }

    /// <summary>
    /// Gets or sets the bounced count.
    /// </summary>
    public int BouncedCount { get; set; }

    /// <summary>
    /// Gets or sets the open count.
    /// </summary>
    public int OpenCount { get; set; }

    /// <summary>
    /// Gets or sets the click count.
    /// </summary>
    public int ClickCount { get; set; }

    /// <summary>
    /// Gets or sets the current message status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last open timestamp.
    /// </summary>
    public string? LastOpenedAt { get; set; }

    /// <summary>
    /// Gets or sets the last click timestamp.
    /// </summary>
    public string? LastClickedAt { get; set; }

    /// <summary>
    /// Gets or sets the last delivery event timestamp.
    /// </summary>
    public string? LastDeliveryEventAt { get; set; }
}

/// <summary>
/// Aggregate tracked message counters.
/// </summary>
public sealed class TrackingSummary
{
    /// <summary>
    /// Gets or sets total tracked messages.
    /// </summary>
    public long Messages { get; set; }

    /// <summary>
    /// Gets or sets total recipients.
    /// </summary>
    public long Recipients { get; set; }

    /// <summary>
    /// Gets or sets total accepted deliveries.
    /// </summary>
    public long Accepted { get; set; }

    /// <summary>
    /// Gets or sets total completed deliveries.
    /// </summary>
    public long Delivered { get; set; }

    /// <summary>
    /// Gets or sets total bounces.
    /// </summary>
    public long Bounced { get; set; }

    /// <summary>
    /// Gets or sets total opens.
    /// </summary>
    public long Opens { get; set; }

    /// <summary>
    /// Gets or sets total clicks.
    /// </summary>
    public long Clicks { get; set; }
}

/// <summary>
/// Response payload for listing tracked outbound messages.
/// </summary>
public sealed class TrackingList
{
    /// <summary>
    /// Gets or sets the tracked messages.
    /// </summary>
    public IReadOnlyList<TrackingMessage> Messages { get; set; } = Array.Empty<TrackingMessage>();

    /// <summary>
    /// Gets or sets the aggregate summary counters.
    /// </summary>
    public TrackingSummary Summary { get; set; } = new();
}

/// <summary>
/// Individual delivery, open, or click event in a message tracking timeline.
/// </summary>
public sealed class TrackingEvent
{
    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipient email address associated with this event.
    /// </summary>
    public string? RecipientEmail { get; set; }

    /// <summary>
    /// Gets or sets the clicked URL if this is a click event.
    /// </summary>
    public string? ClickUrl { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the event occurred.
    /// </summary>
    public string? OccurredAt { get; set; }

    /// <summary>
    /// Gets or sets optional engagement metadata as JSON.
    /// </summary>
    public JsonElement? Engagement { get; set; }
}

/// <summary>
/// Tracked message paired with its timeline events.
/// </summary>
public sealed class TrackingDetail
{
    /// <summary>
    /// Gets or sets the tracked message metadata.
    /// </summary>
    public TrackingMessage Message { get; set; } = new();

    /// <summary>
    /// Gets or sets the timeline events.
    /// </summary>
    public IReadOnlyList<TrackingEvent> Events { get; set; } = Array.Empty<TrackingEvent>();
}

internal sealed class TrackingDetailEnvelope
{
    public TrackingDetail Message { get; set; } = new();
}

/// <summary>
/// Query filters for listing tracked messages.
/// </summary>
public sealed class TrackingListParams
{
    /// <summary>
    /// Gets or sets the maximum number of rows to return.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Gets or sets the pagination offset.
    /// </summary>
    public int? Offset { get; set; }

    /// <summary>
    /// Gets or sets an optional status filter.
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// Individual delivery event inside an email log entry.
/// </summary>
public sealed class EmailLogEvent
{
    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the delivery status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event occurrence timestamp.
    /// </summary>
    public string? OccurredAt { get; set; }

    /// <summary>
    /// Gets or sets the recipient email address.
    /// </summary>
    public string? Recipient { get; set; }

    /// <summary>
    /// Gets or sets diagnostic payload details.
    /// </summary>
    public JsonElement? Diagnostic { get; set; }

    /// <summary>
    /// Gets or sets the event source.
    /// </summary>
    public string? Source { get; set; }
}

/// <summary>
/// Customer-visible delivery audit log entry.
/// </summary>
public sealed class EmailLog
{
    /// <summary>
    /// Gets or sets the log ID if present.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the account ID.
    /// </summary>
    public long? AccountId { get; set; }

    /// <summary>
    /// Gets or sets the message tracking ID.
    /// </summary>
    public string TrackingId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the sender domain.
    /// </summary>
    public string? SenderDomain { get; set; }

    /// <summary>
    /// Gets or sets the sender address.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Gets or sets the sender display name.
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// Gets or sets the submission method (<c>api</c> or <c>smtp</c>).
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// Gets or sets the To recipients.
    /// </summary>
    public IReadOnlyList<string> To { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the CC recipients.
    /// </summary>
    public IReadOnlyList<string> Cc { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the primary recipient string when returned directly.
    /// </summary>
    public string? Recipient { get; set; }

    /// <summary>
    /// Gets or sets when the message was received by the platform.
    /// </summary>
    public string? ReceivedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the delivery status.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the recipient count.
    /// </summary>
    public long RecipientsCount { get; set; }

    /// <summary>
    /// Gets or sets the delivered count.
    /// </summary>
    public long DeliveredCount { get; set; }

    /// <summary>
    /// Gets or sets the bounced count.
    /// </summary>
    public long BouncedCount { get; set; }

    /// <summary>
    /// Gets or sets diagnostic details.
    /// </summary>
    public string? Diagnostic { get; set; }

    /// <summary>
    /// Gets or sets detailed delivery events when available on the plan.
    /// </summary>
    public IReadOnlyList<EmailLogEvent> Events { get; set; } = Array.Empty<EmailLogEvent>();
}

/// <summary>
/// Response payload for account email delivery logs.
/// </summary>
public sealed class EmailLogs
{
    /// <summary>
    /// Gets or sets a value indicating whether email logs are available on the current plan.
    /// </summary>
    public bool Available { get; set; }

    /// <summary>
    /// Gets or sets the log rows.
    /// </summary>
    public IReadOnlyList<EmailLog> Messages { get; set; } = Array.Empty<EmailLog>();

    /// <summary>
    /// Gets or sets a value indicating whether more rows exist.
    /// </summary>
    public bool HasMore { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether per-event diagnostics are available on the plan.
    /// </summary>
    public bool EventDetailsAvailable { get; set; }
}

/// <summary>
/// Query parameters for filtering account email logs.
/// </summary>
public sealed class EmailLogParams
{
    /// <summary>
    /// Gets or sets the maximum number of rows to return.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Gets or sets the pagination offset.
    /// </summary>
    public int? Offset { get; set; }

    /// <summary>
    /// Gets or sets an optional status filter.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets an optional start date/time filter.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Gets or sets an optional end date/time or recipient filter.
    /// </summary>
    public string? To { get; set; }
}
