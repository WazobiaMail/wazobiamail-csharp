using System.Net;
using System.Text.Json;
using Xunit;

namespace WazobiaMail.Tests;

public sealed class DomainsTests
{
    [Fact]
    public async Task DomainCrudDnsVerifyAndCloudflareEndpoints_WorkAsExpected()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"ok","data":{"items":[{"id":"dom_1","accountId":1,"domain":"mail.acme.com","verificationStatus":"verified","isDefault":true,"domainRole":"sending","mailMode":"hosted"}]}}""");
        handler.EnqueueJson(HttpStatusCode.Created, """{"success":true,"message":"created","data":{"domain":{"id":"dom_2","accountId":1,"domain":"acme.com","verificationStatus":"pending","isDefault":false,"domainRole":"primary","mailMode":"hosted"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"fetched","data":{"domain":{"id":"dom_2","domain":"acme.com","verificationStatus":"pending"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"updated","data":{"domain":{"id":"dom_2","domain":"acme.com","isDefault":true}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"dns","data":{"domain":{"provisioning":false,"domainId":"dom_2","domain":"acme.com","verificationStatus":"pending","records":[{"id":"rec_1","recordType":"TXT","host":"_dmarc.acme.com","value":"v=DMARC1; p=none","ttl":3600,"status":"pending"}],"copyBlock":"..."}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"verified","data":{"verification":{"domainId":"dom_2","domain":"acme.com","verificationStatus":"verified","verifiedCount":3,"pendingCount":0,"checks":[{"recordType":"TXT","host":"acme.com","expectedValue":"v=spf1","matched":true,"observedValues":["v=spf1"]}]}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"refreshed","data":{"verification":{"domainId":"dom_2","verificationStatus":"verified","verifiedCount":3,"pendingCount":0,"checks":[]}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"cf","data":{"connected":true,"zoneId":"z_1","domain":"acme.com"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"connect","data":{"authorizationUrl":"https://dash.cloudflare.com/oauth"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"synced","data":{"zone":"acme.com","added":2,"updated":1,"records":3,"verificationStatus":"verified"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"disconnected","data":{"disconnected":true}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"deleted","data":{"domainId":"dom_2"}}""");

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_test_12345678" }, http);

        var list = await client.Domains.ListAsync();
        Assert.True(Assert.Single(list.Data.Items).IsVerified);

        var created = await client.Domains.CreateAsync(new CreateDomainRequest { Domain = "acme.com", MailMode = "hosted" });
        Assert.Equal("dom_2", created.Data.Domain.Id);

        var fetched = await client.Domains.GetAsync("dom_2");
        Assert.Equal("acme.com", fetched.Data.Domain.DomainName);

        var updated = await client.Domains.UpdateAsync("dom_2", new UpdateDomainRequest { IsDefault = true });
        Assert.True(updated.Data.Domain.IsDefault);

        var dns = await client.Domains.GetDnsRecordsAsync("dom_2");
        Assert.Single(dns.Data.Domain.Records);

        var verified = await client.Domains.VerifyAsync("dom_2", force: true);
        Assert.Equal(3, verified.Data.Verification.VerifiedCount);

        var refreshed = await client.Domains.RefreshDnsAsync("dom_2");
        Assert.Equal("verified", refreshed.Data.Verification.VerificationStatus);

        var cfStatus = await client.Domains.GetCloudflareStatusAsync("dom_2");
        Assert.True(cfStatus.Data.Connected);

        var cfConnect = await client.Domains.ConnectCloudflareAsync("dom_2");
        Assert.StartsWith("https://dash.cloudflare.com", cfConnect.Data.AuthorizationUrl, StringComparison.Ordinal);

        var cfSync = await client.Domains.SyncCloudflareAsync("dom_2");
        Assert.Equal(3, cfSync.Data.Records);

        var cfDisconnect = await client.Domains.DisconnectCloudflareAsync("dom_2");
        Assert.True(cfDisconnect.Data.GetProperty("disconnected").GetBoolean());

        var deleted = await client.Domains.DeleteAsync("dom_2");
        Assert.Equal("dom_2", deleted.Data.DomainId);

        Assert.Equal(HttpMethod.Get, handler.Requests[8].Method); // cloudflare/connect must be GET
        Assert.Equal("?domainId=dom_2", handler.Requests[8].Uri.Query);
        using var verifyBody = JsonDocument.Parse(handler.Requests[5].BodyBytes);
        Assert.True(verifyBody.RootElement.GetProperty("force").GetBoolean());
    }
}
