using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace WazobiaMail.Tests;

public sealed class SupportAndTemplatesTests
{
    [Fact]
    public async Task SupportCreate_AutoFillsFromAuthMeWhenOmitted_AndThrowsIfMissing()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"me","data":{"accountId":7,"email":"owner@acme.com","name":"Ada Lovelace"}}""");
        handler.EnqueueJson(HttpStatusCode.Created, """{"success":true,"message":"created","data":{"id":"tkt_1","ticketId":"TKT-1001"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"me","data":{"accountId":7,"email":"","name":""}}""");

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_test_12345678" }, http);

        var created = await client.Support.CreateAsync(new CreateSupportTicketRequest
        {
            Subject = "DKIM help",
            Message = "Need assistance"
        });
        Assert.Equal("TKT-1001", created.Data.TicketId);

        using (var postDoc = JsonDocument.Parse(handler.Requests[1].BodyBytes))
        {
            Assert.Equal("Ada Lovelace", postDoc.RootElement.GetProperty("name").GetString());
            Assert.Equal("owner@acme.com", postDoc.RootElement.GetProperty("email").GetString());
        }

        await Assert.ThrowsAsync<WazobiaValidationException>(() => client.Support.CreateAsync(new CreateSupportTicketRequest
        {
            Subject = "Missing identity",
            Message = "Hello"
        }));
    }

    [Fact]
    public async Task SupportListGetReplyMarkReadDownload_AndTemplatesCrud_WorkAsExpected()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"list","data":{"items":[{"id":"tkt_1","subject":"DKIM help","status":"open"}],"total":1,"page":1,"pages":1}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"get","data":{"id":"tkt_1","subject":"DKIM help","messages":[{"id":"m_1","message":"Initial"}]}}""");
        handler.EnqueueJson(HttpStatusCode.Created, """{"success":true,"message":"rep","data":{"id":"m_2","message":"Thanks"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"read","data":{"read":true}}""");
        handler.EnqueueBytes(HttpStatusCode.OK, Encoding.UTF8.GetBytes("log lines"), "text/plain");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"tlist","data":{"items":[{"id":"tpl_1","name":"Welcome","channel":"email","status":"approved"}],"total":1,"limit":10,"offset":0}}""");
        handler.EnqueueJson(HttpStatusCode.Created, """{"success":true,"message":"tcreate","data":{"id":"tpl_2","name":"Receipt","channel":"email"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"tget","data":{"id":"tpl_2","name":"Receipt"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"tupd","data":{"id":"tpl_2","name":"Receipt v2"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"tprev","data":{"channel":"email","subject":"Hi Ada","renderedBody":"<p>Hi Ada</p>","canSend":true}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"tsync","data":{"id":"tpl_2","status":"approved"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"tdel","data":{"templateId":"tpl_2"}}""");

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_test_12345678" }, http);

        var tickets = await client.Support.ListAsync(new SupportListParams { Status = "open", Page = 1, Limit = 10, Descending = true });
        Assert.Single(tickets.Data.Items);

        var ticket = await client.Support.GetAsync("tkt_1");
        Assert.Single(ticket.Data.Messages);

        var reply = await client.Support.ReplyAsync("tkt_1", new ReplySupportTicketRequest { Message = "Thanks" });
        Assert.Equal("m_2", reply.Data.Id);

        var marked = await client.Support.MarkReadAsync("tkt_1");
        Assert.True(marked.Data.GetProperty("read").GetBoolean());

        var attachment = await client.Support.DownloadAttachmentAsync("att_1");
        Assert.Equal("log lines", Encoding.UTF8.GetString(attachment.Content));

        var templates = await client.Templates.ListAsync(new TemplateListParams { Channel = "email", Limit = 10 });
        Assert.Single(templates.Data.Items);

        var tpl = await client.Templates.CreateAsync(new CreateTemplateRequest { Name = "Receipt", Body = "<p>Hi {{name}}</p>", Subject = "Hi {{name}}" });
        Assert.Equal("tpl_2", tpl.Data.Id);

        Assert.Equal("Receipt", (await client.Templates.GetAsync("tpl_2")).Data.Name);
        Assert.Equal("Receipt v2", (await client.Templates.UpdateAsync("tpl_2", new UpdateTemplateRequest { Name = "Receipt v2" })).Data.Name);

        var preview = await client.Templates.PreviewAsync("tpl_2", new PreviewTemplateRequest
        {
            SampleVariables = new Dictionary<string, object?> { ["name"] = "Ada" }
        });
        Assert.Equal("<p>Hi Ada</p>", preview.Data.RenderedBody);

        var synced = await client.Templates.SyncMetaAsync("tpl_2");
        Assert.Equal("approved", synced.Data.Status);

        var deleted = await client.Templates.DeleteAsync("tpl_2");
        Assert.Equal("tpl_2", deleted.Data["templateId"]);
    }
}
