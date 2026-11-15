using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Transactional email sending, attachments, spam checking, tracking, and delivery logs.
/// </summary>
public sealed class MessagesResource
{
    private readonly WazobiaHttpTransport _transport;

    internal MessagesResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Sends a transactional email message.
    /// </summary>
    public Task<ApiResponse<SendEnvelope>> SendAsync(
        SendMessageRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var payload = BuildSendPayload(request);
        var effectiveOptions = MergeIdempotencyKey(request.IdempotencyKey, options);
        return _transport.SendJsonAsync<SendEnvelope>(
            HttpMethod.Post,
            "/sending/messages",
            payload,
            options: effectiveOptions,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sends a transactional email message with direct binary multipart attachments.
    /// </summary>
    public async Task<ApiResponse<SendEnvelope>> SendWithAttachmentsAsync(
        SendMessageRequest request,
        IEnumerable<FileUpload> files,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(files);

        var payload = BuildSendPayload(request);
        var messageJson = JsonSerializer.Serialize(payload, WazobiaHttpTransport.JsonOptions);

        using var multipart = new MultipartFormDataContent();
        var messageContent = new StringContent(messageJson, Encoding.UTF8, "application/json");
        messageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "\"message\""
        };
        multipart.Add(messageContent);

        var fileList = files as ICollection<FileUpload> ?? files.ToList();
        if (fileList.Count == 0)
        {
            throw new ArgumentException("At least one file attachment is required.", nameof(files));
        }

        foreach (var file in fileList)
        {
            ArgumentNullException.ThrowIfNull(file);
            multipart.Add(BuildFilePart(file, "files"));
        }

        var effectiveOptions = MergeIdempotencyKey(request.IdempotencyKey, options);
        return await _transport.SendMultipartAsync<SendEnvelope>(
            "/sending/messages/with-attachments",
            multipart,
            effectiveOptions,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Uploads an attachment file and returns its public CDN URL for use in <see cref="UrlAttachment"/>.
    /// </summary>
    public async Task<ApiResponse<UploadedFileEnvelope>> UploadAttachmentAsync(
        FileUpload file,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        using var multipart = new MultipartFormDataContent();
        multipart.Add(BuildFilePart(file, "file"));

        return await _transport.SendMultipartAsync<UploadedFileEnvelope>(
            "/sending/attachments/upload",
            multipart,
            options,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs a pre-flight spam and deliverability check on an email draft.
    /// </summary>
    public Task<ApiResponse<SpamCheckResult>> SpamCheckAsync(
        SpamCheckRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<SpamCheckResult>(
            HttpMethod.Post,
            "/sending/spam-check",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Lists tracked outbound email messages and aggregate engagement counters.
    /// </summary>
    public Task<ApiResponse<TrackingList>> ListTrackedAsync(
        TrackingListParams? parameters = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        if (parameters?.Limit is > 0)
        {
            query["limit"] = parameters.Limit.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (parameters?.Offset is > 0)
        {
            query["offset"] = parameters.Offset.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrWhiteSpace(parameters?.Status))
        {
            query["status"] = parameters.Status;
        }

        return _transport.SendJsonAsync<TrackingList>(
            HttpMethod.Get,
            "/sending/tracking/messages",
            query: query,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves full delivery, open, and click timeline events for a tracked message.
    /// </summary>
    public async Task<ApiResponse<TrackingDetail>> GetTrackedAsync(
        string trackingId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingId);
        var envelope = await _transport.SendJsonAsync<TrackingDetailEnvelope>(
            HttpMethod.Get,
            $"/sending/tracking/messages/{Uri.EscapeDataString(trackingId)}",
            options: options,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return new ApiResponse<TrackingDetail>
        {
            Success = envelope.Success,
            Message = envelope.Message,
            Data = envelope.Data?.Message ?? new TrackingDetail()
        };
    }

    /// <summary>
    /// Lists customer-visible email delivery audit logs.
    /// </summary>
    public Task<ApiResponse<EmailLogs>> ListEmailLogsAsync(
        EmailLogParams? parameters = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        if (parameters?.Limit is > 0)
        {
            query["limit"] = parameters.Limit.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (parameters?.Offset is > 0)
        {
            query["offset"] = parameters.Offset.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrWhiteSpace(parameters?.Status))
        {
            query["status"] = parameters.Status;
        }

        if (!string.IsNullOrWhiteSpace(parameters?.From))
        {
            query["from"] = parameters.From;
        }

        if (!string.IsNullOrWhiteSpace(parameters?.To))
        {
            query["to"] = parameters.To;
        }

        return _transport.SendJsonAsync<EmailLogs>(
            HttpMethod.Get,
            "/account/email-logs",
            query: query,
            options: options,
            cancellationToken: cancellationToken);
    }

    private static SendMessagePayload BuildSendPayload(SendMessageRequest request)
    {
        return new SendMessagePayload
        {
            To = request.To.ToArray(),
            Cc = request.Cc.Count > 0 ? request.Cc.ToArray() : null,
            Bcc = request.Bcc.Count > 0 ? request.Bcc.ToArray() : null,
            Content = new SendContentPayload
            {
                From = string.IsNullOrWhiteSpace(request.From) ? null : request.From,
                Subject = request.Subject,
                TextBody = string.IsNullOrWhiteSpace(request.Text) ? null : request.Text,
                HtmlBody = string.IsNullOrWhiteSpace(request.Html) ? null : request.Html,
                ReplyTo = string.IsNullOrWhiteSpace(request.ReplyTo) ? null : request.ReplyTo
            },
            EnvelopeSender = string.IsNullOrWhiteSpace(request.EnvelopeSender) ? null : request.EnvelopeSender,
            Attachments = request.Attachments.Count > 0 ? request.Attachments.ToArray() : null
        };
    }

    private static ByteArrayContent BuildFilePart(FileUpload file, string fieldName)
    {
        if (file.Content is null || file.Content.Length == 0)
        {
            throw new ArgumentException("Attachment content must not be empty.", nameof(file));
        }

        var safeFileName = (file.FileName ?? string.Empty)
            .Replace("\r", "_")
            .Replace("\n", "_")
            .Replace("\0", "_")
            .Replace("\"", "_")
            .Replace("\\", "_")
            .Replace("/", "_")
            .Trim();
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            safeFileName = "attachment.dat";
        }

        var fileContent = new ByteArrayContent(file.Content);
        var cleanMime = (file.ContentType ?? string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\0", string.Empty)
            .Trim();
        fileContent.Headers.ContentType = MediaTypeHeaderValue.TryParse(cleanMime, out var parsedMediaType)
            ? parsedMediaType
            : new MediaTypeHeaderValue("application/octet-stream");
        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = $"\"{fieldName}\"",
            FileName = $"\"{safeFileName}\""
        };
        return fileContent;
    }

    private static RequestOptions? MergeIdempotencyKey(string? requestIdempotencyKey, RequestOptions? options)
    {
        if (!string.IsNullOrWhiteSpace(options?.IdempotencyKey) || string.IsNullOrWhiteSpace(requestIdempotencyKey))
        {
            return options;
        }

        return new RequestOptions
        {
            IdempotencyKey = requestIdempotencyKey,
            Headers = options?.Headers
        };
    }
}
