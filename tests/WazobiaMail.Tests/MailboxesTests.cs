using System.Net;
using Xunit;

namespace WazobiaMail.Tests;

public sealed class MailboxesTests
{
    [Fact]
    public async Task MailboxesAndHeadlessInboxOperations_WorkAsExpected()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"ok","data":[{"id":"mb_1","domainId":"dom_1","address":"support@acme.com","localPart":"support","status":"active","role":"general"}]}""");
        handler.EnqueueJson(HttpStatusCode.Created, """{"success":true,"message":"created","data":{"mailbox":{"id":"mb_2","domainId":"dom_1","address":"sales@acme.com","localPart":"sales","status":"active","role":"general"},"credential":{"credential":{"id":"cred_1","mailboxId":"mb_2","username":"sales@acme.com"},"password":"one-time-pass"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"get","data":{"mailbox":{"id":"mb_2","address":"sales@acme.com","status":"active"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"upd","data":{"mailbox":{"id":"mb_2","displayName":"Sales Team"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"conn","data":{"mailboxId":"mb_2","domainId":"dom_1","username":"sales@acme.com","status":"active","smtp":{"host":"smtp.wazobiamail.com","port":587},"authentication":"PLAIN"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"storage","data":{"mailboxes":[{"id":"mb_2","address":"sales@acme.com","usedBytes":1024,"quotaBytes":1048576}],"measurement":"measured","plan":{"mailboxLimit":5,"mailboxesUsed":2}}}""");
        handler.EnqueueJson(HttpStatusCode.Created, """{"success":true,"message":"cred","data":{"credential":{"credential":{"id":"cred_2","mailboxId":"mb_2","username":"sales@acme.com"},"password":"secret-2"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"rot","data":{"credential":{"credential":{"id":"cred_3","mailboxId":"mb_2","username":"sales@acme.com"},"password":"secret-3"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"rev","data":{"mailboxId":"mb_2","credentialId":"cred_2"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"msgs","data":{"items":[{"id":"msg_1","subject":"Hello","from":[{"email":"lead@client.com"}],"to":[{"email":"sales@acme.com"}],"isUnread":true,"hasAttachments":false,"sizeBytes":512}],"total":1,"page":1,"limit":25}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"msg","data":{"id":"msg_1","subject":"Hello","htmlBody":"<p>Hi</p>","textBody":"Hi","attachments":[{"blobId":"b_1","name":"a.txt","type":"text/plain","sizeBytes":2}]}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"delmsg","data":{"messageId":"msg_1"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"del","data":{"mailboxId":"mb_2"}}""");

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_test_12345678" }, http);

        var list = await client.Mailboxes.ListAsync();
        Assert.Equal("support@acme.com", Assert.Single(list.Data).Address);

        var created = await client.Mailboxes.CreateAsync(new CreateMailboxRequest { DomainId = "dom_1", LocalPart = "sales" });
        Assert.Equal("one-time-pass", created.Data.Credential.Password);

        var got = await client.Mailboxes.GetAsync("mb_2");
        Assert.Equal("sales@acme.com", got.Data.Mailbox.Address);

        var updated = await client.Mailboxes.UpdateAsync("mb_2", new UpdateMailboxRequest { DisplayName = "Sales Team" });
        Assert.Equal("Sales Team", updated.Data.Mailbox.DisplayName);

        var conn = await client.Mailboxes.GetConnectionAsync("mb_2");
        Assert.Equal("smtp.wazobiamail.com", conn.Data.Smtp?.GetProperty("host").GetString());

        var storage = await client.Mailboxes.GetStorageAsync();
        Assert.Equal(5, storage.Data.Plan?.MailboxLimit);

        var cred = await client.Mailboxes.CreateCredentialAsync("mb_2");
        Assert.Equal("secret-2", cred.Data.Credential.Password);

        var rotated = await client.Mailboxes.RotateCredentialAsync("mb_2");
        Assert.Equal("secret-3", rotated.Data.Credential.Password);

        var revoked = await client.Mailboxes.RevokeCredentialAsync("mb_2", "cred_2");
        Assert.Equal("cred_2", revoked.Data.CredentialId);

        var msgs = await client.Mailboxes.ListMessagesAsync("mb_2", new ListMessagesParams { Page = 1, Limit = 25, UnreadOnly = true, Search = "Hello" });
        Assert.Single(msgs.Data.Items);
        Assert.Contains("unreadOnly=true", handler.Requests[9].Uri.Query, StringComparison.Ordinal);

        var msgDetail = await client.Mailboxes.GetMessageAsync("mb_2", "msg_1");
        Assert.Equal("<p>Hi</p>", msgDetail.Data.HtmlBody);

        var delMsg = await client.Mailboxes.DeleteMessageAsync("mb_2", "msg_1");
        Assert.Equal("msg_1", delMsg.Data.MessageId);

        var delMb = await client.Mailboxes.DeleteAsync("mb_2");
        Assert.Equal("mb_2", delMb.Data.MailboxId);
    }
}
