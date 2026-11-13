using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace WazobiaMail.Tests;

public sealed class MessagesTests
{
    [Fact]
    public async Task SendAsync_SerializesCanonicalNestedPayload()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(
            HttpStatusCode.OK,
            """{"success":true,"message":"Message accepted","data":{"result":{"status":"queued","trackingId":"trk_abc","recipients":2}}}""");

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_test_12345678" }, http);

        var response = await client.Messages.SendAsync(new SendMessageRequest
        {
            From = "Acme <hello@acme.test>",
            To = ["alice@example.com", "bob@example.com"],
            Cc = ["cc@example.com"],
            Bcc = ["bcc@example.com"],
            Subject = "Welcome",
            Html = "<p>Hello</p>",
            Text = "Hello",
            ReplyTo = "reply@acme.test",
            EnvelopeSender = "bounces@acme.test",
            Attachments = [new UrlAttachment("https://cdn.acme.test/doc.pdf", "doc.pdf")],
            IdempotencyKey = "msg-idem-1"
        });

        Assert.True(response.Success);
        Assert.Equal("trk_abc", response.Data.Result.TrackingId);
        Assert.Equal(2, response.Data.Result.Recipients);

        var req = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Equal("/api/v1/sending/messages", req.Uri.AbsolutePath);
        Assert.Equal("msg-idem-1", req.Headers["Idempotency-Key"].Single());

        using var doc = JsonDocument.Parse(req.BodyBytes);
        var root = doc.RootElement;
        Assert.Equal("alice@example.com", root.GetProperty("to")[0].GetString());
        Assert.Equal("cc@example.com", root.GetProperty("cc")[0].GetString());
        Assert.Equal("bcc@example.com", root.GetProperty("bcc")[0].GetString());
        Assert.Equal("bounces@acme.test", root.GetProperty("envelopeSender").GetString());
        Assert.Equal("doc.pdf", root.GetProperty("attachments")[0].GetProperty("fileName").GetString());

        var content = root.GetProperty("content");
        Assert.Equal("Acme <hello@acme.test>", content.GetProperty("from").GetString());
        Assert.Equal("Welcome", content.GetProperty("subject").GetString());
        Assert.Equal("<p>Hello</p>", content.GetProperty("htmlBody").GetString());
        Assert.Equal("Hello", content.GetProperty("textBody").GetString());
        Assert.Equal("reply@acme.test", content.GetProperty("replyTo").GetString());
    }

    [Fact]
    public async Task SendWithAttachmentsAsync_SendsMultipartMessageAndFilesParts()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(
            HttpStatusCode.OK,
            """{"success":true,"message":"Sent","data":{"result":{"status":"queued","trackingId":"trk_mp","recipients":1}}}""");

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_test_12345678" }, http);

        var result = await client.Messages.SendWithAttachmentsAsync(
            new SendMessageRequest
            {
                From = "hello@acme.test",
                To = ["dest@example.com"],
                Subject = "Invoice",
                Text = "Attached.",
                IdempotencyKey = "mp-idem-9"
            },
            [new FileUpload("invoice.pdf", Encoding.UTF8.GetBytes("%PDF-1.4"), "application/pdf")]);

        Assert.Equal("trk_mp", result.Data.Result.TrackingId);

        var req = Assert.Single(handler.Requests);
        Assert.Equal("/api/v1/sending/messages/with-attachments", req.Uri.AbsolutePath);
        Assert.Equal("mp-idem-9", req.Headers["Idempotency-Key"].Single());
        Assert.StartsWith("multipart/form-data", req.ContentType, StringComparison.OrdinalIgnoreCase);

        var bodyText = Encoding.UTF8.GetString(req.BodyBytes);
        Assert.Contains("name=\"message\"", bodyText, StringComparison.Ordinal);
        Assert.Contains("name=\"files\"; filename=\"invoice.pdf\"", bodyText, StringComparison.Ordinal);
        Assert.Contains("%PDF-1.4", bodyText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UploadAttachmentAndSpamCheckAndTrackingAndLogs_WorkAsExpected()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(
            HttpStatusCode.OK,
            """{"success":true,"message":"Uploaded","data":{"file":{"fileName":"logo.png","contentType":"image/png","sizeBytes":4,"publicUrl":"https://cdn.wazobiamail.com/logo.png"}}}""");
        handler.EnqueueJson(
            HttpStatusCode.OK,
            """{"success":true,"message":"Checked","data":{"score":1.5,"threshold":5.0,"isSpam":false,"rating":"Great","badgeColor":"green","deliverabilityScore":92,"summary":"Clean","rules":[{"code":"SPF_PASS","points":-1.0,"title":"SPF","description":"Valid","recommendation":"None"}]}}""");
        handler.EnqueueJson(
            HttpStatusCode.OK,
            """{"success":true,"message":"Tracked","data":{"messages":[{"trackingId":"trk_1","senderDomain":"acme.test","recipientsCount":1,"acceptedCount":1,"deliveredCount":1,"bouncedCount":0,"openCount":2,"clickCount":1,"status":"delivered"}],"summary":{"messages":1,"recipients":1,"accepted":1,"delivered":1,"bounced":0,"opens":2,"clicks":1}}}""");
        handler.EnqueueJson(
            HttpStatusCode.OK,
            """{"success":true,"message":"Detail","data":{"message":{"message":{"trackingId":"trk_1","senderDomain":"acme.test","status":"delivered"},"events":[{"id":"evt_1","eventType":"open","recipientEmail":"user@example.com","occurredAt":"2026-11-12T10:00:00Z"}]}}}""");
        handler.EnqueueJson(
            HttpStatusCode.OK,
            """{"success":true,"message":"Logs","data":{"available":true,"messages":[{"trackingId":"trk_1","status":"delivered","to":["user@example.com"]}],"hasMore":false,"eventDetailsAvailable":true}}""");

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_test_12345678" }, http);

        var uploaded = await client.Messages.UploadAttachmentAsync(new FileUpload("logo.png", [1, 2, 3, 4], "image/png"));
        Assert.Equal("https://cdn.wazobiamail.com/logo.png", uploaded.Data.File.PublicUrl);

        var spam = await client.Messages.SpamCheckAsync(new SpamCheckRequest { Subject = "Order update", Text = "Your order shipped." });
        Assert.False(spam.Data.IsSpam);
        Assert.Equal(92, spam.Data.DeliverabilityScore);

        var tracked = await client.Messages.ListTrackedAsync(new TrackingListParams { Limit = 10, Offset = 5, Status = "delivered" });
        Assert.Single(tracked.Data.Messages);
        Assert.Equal(2, tracked.Data.Summary.Opens);

        var detail = await client.Messages.GetTrackedAsync("trk_1");
        Assert.Equal("trk_1", detail.Data.Message.TrackingId);
        Assert.Equal("open", Assert.Single(detail.Data.Events).EventType);

        var logs = await client.Messages.ListEmailLogsAsync(new EmailLogParams { Limit = 20, Status = "delivered", To = "user@example.com" });
        Assert.True(logs.Data.Available);
        Assert.Single(logs.Data.Messages);

        Assert.Contains("limit=10", handler.Requests[2].Uri.Query, StringComparison.Ordinal);
        Assert.Contains("offset=5", handler.Requests[2].Uri.Query, StringComparison.Ordinal);
        Assert.Contains("status=delivered", handler.Requests[2].Uri.Query, StringComparison.Ordinal);
        Assert.Contains("to=user%40example.com", handler.Requests[4].Uri.Query, StringComparison.Ordinal);
    }
}
