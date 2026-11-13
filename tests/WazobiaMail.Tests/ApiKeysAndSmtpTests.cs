using System.Net;
using System.Text.Json;
using Xunit;

namespace WazobiaMail.Tests;

public sealed class ApiKeysAndSmtpTests
{
    [Fact]
    public async Task ApiKeysCrud_WorksAsExpected()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"ok","data":{"items":[{"id":"key_1","name":"Prod","scope":"sending","keyPrefix":"wzbm_live"}]}}""");
        handler.EnqueueJson(HttpStatusCode.Created, """{"success":true,"message":"created","data":{"apiKey":{"id":"key_2","name":"CI","scope":"sending","keyPrefix":"wzbm_live","apiKey":"wzbm_live_secret_2"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"regen","data":{"apiKey":{"id":"key_2","name":"CI","scope":"sending","keyPrefix":"wzbm_live","apiKey":"wzbm_live_secret_3"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"revoked","data":{"keyId":"key_2"}}""");

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_test_12345678" }, http);

        var list = await client.ApiKeys.ListAsync();
        Assert.Equal("Prod", Assert.Single(list.Data.Items).Name);

        var created = await client.ApiKeys.CreateAsync(new CreateApiKeyRequest { Name = "CI", Scope = "sending" });
        Assert.Equal("wzbm_live_secret_2", created.Data.ApiKey.ApiKey);

        var regenerated = await client.ApiKeys.RegenerateAsync("key_2");
        Assert.Equal("wzbm_live_secret_3", regenerated.Data.ApiKey.ApiKey);

        var revoked = await client.ApiKeys.RevokeAsync("key_2");
        Assert.Equal("key_2", revoked.Data["keyId"]);
        Assert.Equal("/api/v1/sending/api-keys/key_2/revoke", handler.Requests[3].Uri.AbsolutePath);
    }

    [Fact]
    public async Task SmtpRelaysAndAccess_SerializeSnakeCaseAndDeserializeCamelCase()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"list","data":{"smtpRelays":[{"id":"rel_1","name":"Primary","smtpHost":"smtp.relay.test","smtpPort":587,"smtpUsername":"u","smtpSecurity":"starttls","status":"active","isDefault":true,"hasPassword":true}]}}""");
        handler.EnqueueJson(HttpStatusCode.Created, """{"success":true,"message":"created","data":{"smtpRelay":{"id":"rel_2","name":"Backup","smtpHost":"smtp2.relay.test","smtpPort":465,"smtpUsername":"u2","smtpSecurity":"tls","status":"inactive","isDefault":false,"hasPassword":true}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"get","data":{"smtpRelay":{"id":"rel_2","name":"Backup"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"upd","data":{"smtpRelay":{"id":"rel_2","name":"Backup Updated"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"act","data":{"smtpRelay":{"id":"rel_2","status":"active"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"deact","data":{"smtpRelay":{"id":"rel_2","status":"inactive"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"del","data":{"relayId":"rel_2"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"acc","data":{"paid":true,"eligible":true,"enabled":true,"configured":true,"connection":{"host":"smtp.wazobiamail.com","port":587,"security":"starttls","usernames":["acct@acme.test"]}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"updacc","data":{"enabled":false}}""");

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_test_12345678" }, http);

        var relays = await client.Smtp.ListRelaysAsync();
        Assert.Equal("smtp.relay.test", Assert.Single(relays.Data.SmtpRelays).SmtpHost);

        var created = await client.Smtp.CreateRelayAsync(new CreateSmtpRelayRequest
        {
            Name = "Backup",
            SmtpHost = "smtp2.relay.test",
            SmtpPort = 465,
            SmtpUsername = "u2",
            SmtpPassword = "pw",
            SmtpSecurity = "tls",
            IsDefault = false
        });
        Assert.Equal("rel_2", created.Data.SmtpRelay.Id);

        using (var createDoc = JsonDocument.Parse(handler.Requests[1].BodyBytes))
        {
            var root = createDoc.RootElement;
            Assert.Equal("smtp2.relay.test", root.GetProperty("smtp_host").GetString());
            Assert.Equal(465, root.GetProperty("smtp_port").GetInt32());
            Assert.Equal("u2", root.GetProperty("smtp_username").GetString());
            Assert.Equal("pw", root.GetProperty("smtp_password").GetString());
            Assert.Equal("tls", root.GetProperty("smtp_security").GetString());
            Assert.False(root.GetProperty("is_default").GetBoolean());
        }

        Assert.Equal("rel_2", (await client.Smtp.GetRelayAsync("rel_2")).Data.SmtpRelay.Id);
        Assert.Equal("Backup Updated", (await client.Smtp.UpdateRelayAsync("rel_2", new UpdateSmtpRelayRequest { Name = "Backup Updated" })).Data.SmtpRelay.Name);
        Assert.Equal("active", (await client.Smtp.ActivateRelayAsync("rel_2")).Data.SmtpRelay.Status);
        Assert.Equal("inactive", (await client.Smtp.DeactivateRelayAsync("rel_2")).Data.SmtpRelay.Status);
        Assert.Equal("rel_2", (await client.Smtp.DeleteRelayAsync("rel_2")).Data["relayId"]);

        var access = await client.Smtp.GetAccessAsync();
        Assert.True(access.Data.Enabled);
        Assert.Equal(587, access.Data.Connection?.Port);

        var updatedAccess = await client.Smtp.UpdateAccessAsync(false);
        Assert.False(updatedAccess.Data["enabled"]);
    }
}
