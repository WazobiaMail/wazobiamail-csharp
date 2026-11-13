using System.Net;
using Xunit;

namespace WazobiaMail.Tests;

public sealed class ClientTests
{
    [Fact]
    public void Constructor_UsesEnvironmentFallback_WithPriorityOrder()
    {
        var prevPrimary = Environment.GetEnvironmentVariable("WAZOBIA_API_KEY");
        var prevSecondary = Environment.GetEnvironmentVariable("WAZOBIAMAIL_API_KEY");
        try
        {
            Environment.SetEnvironmentVariable("WAZOBIA_API_KEY", null);
            Environment.SetEnvironmentVariable("WAZOBIAMAIL_API_KEY", "wzbm_secondary_12345678");

            using var secondaryClient = new WazobiaMailClient();
            Assert.Contains("wzbm...5678", secondaryClient.ToString(), StringComparison.Ordinal);

            Environment.SetEnvironmentVariable("WAZOBIA_API_KEY", "wzbm_primary_abcdef12");
            using var primaryClient = new WazobiaMailClient();
            Assert.Contains("wzbm...ef12", primaryClient.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain("abcdef12", primaryClient.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("WAZOBIA_API_KEY", prevPrimary);
            Environment.SetEnvironmentVariable("WAZOBIAMAIL_API_KEY", prevSecondary);
        }
    }

    [Fact]
    public void Constructor_ThrowsAuthenticationException_WhenNoCredentialsProvided()
    {
        var prevPrimary = Environment.GetEnvironmentVariable("WAZOBIA_API_KEY");
        var prevSecondary = Environment.GetEnvironmentVariable("WAZOBIAMAIL_API_KEY");
        try
        {
            Environment.SetEnvironmentVariable("WAZOBIA_API_KEY", null);
            Environment.SetEnvironmentVariable("WAZOBIAMAIL_API_KEY", null);

            Assert.Throws<WazobiaAuthenticationException>(() => new WazobiaMailClient());
        }
        finally
        {
            Environment.SetEnvironmentVariable("WAZOBIA_API_KEY", prevPrimary);
            Environment.SetEnvironmentVariable("WAZOBIAMAIL_API_KEY", prevSecondary);
        }
    }

    [Fact]
    public void Constructor_ValidatesBaseUrlAndTimeout()
    {
        Assert.Throws<ArgumentException>(() =>
            new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_1234567890", BaseUrl = "ftp://bad.example.com" }));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_1234567890", Timeout = TimeSpan.Zero }));

        using var client = new WazobiaMailClient(new WazobiaMailOptions
        {
            ApiKey = "wzbm_1234567890",
            BaseUrl = "https://sandbox.wazobiamail.com/api/v1///"
        });
        Assert.Equal("https://sandbox.wazobiamail.com/api/v1", client.BaseUrl);
    }

    [Fact]
    public void ToString_MasksSensitiveSecrets()
    {
        var options = new WazobiaMailOptions
        {
            ApiKey = "wzbm_live_supersecret_9988",
            AccessToken = "short"
        };
        var optionsStr = options.ToString();
        Assert.Contains("wzbm...9988", optionsStr, StringComparison.Ordinal);
        Assert.DoesNotContain("supersecret", optionsStr, StringComparison.Ordinal);
        Assert.Contains("AccessToken = ***", optionsStr, StringComparison.Ordinal);

        using var client = new WazobiaMailClient(options);
        var clientStr = client.ToString();
        Assert.Contains("wzbm...9988", clientStr, StringComparison.Ordinal);
        Assert.DoesNotContain("supersecret", clientStr, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Dispose_DoesNotDisposeInjectedHttpClient()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"ok","data":{"plan":{},"units":{}}}""");

        using var injected = new HttpClient(handler);
        var client = new WazobiaMailClient(injected, new WazobiaMailOptions { AllowUnauthenticated = true });
        client.Dispose();

        // Injected HttpClient must remain usable after client.Dispose().
        var response = await injected.GetAsync("https://example.com/ping");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Transport_SendsAuthHeadersUserAgentAndPerRequestOverrides()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"Queued","data":{"result":{"status":"queued","trackingId":"trk_1","recipients":1}}}""");

        var options = new WazobiaMailOptions
        {
            ApiKey = "wzbm_live_testkey_1234",
            AccessToken = "jwt_token_value",
            UserAgent = "custom-agent/2.0"
        };
        options.DefaultHeaders["X-Default"] = "initial";

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(options, http);

        var receipt = await client.SendAsync(
            new SendMessageRequest
            {
                From = "Acme <hi@acme.test>",
                To = ["user@example.com"],
                Subject = "Hello",
                Text = "Hi there"
            },
            new RequestOptions
            {
                IdempotencyKey = "idem-123",
                Headers = new Dictionary<string, string> { ["X-Default"] = "overridden", ["X-Trace"] = "abc" }
            });

        Assert.True(receipt.Success);
        Assert.Equal("trk_1", receipt.Data.Result.TrackingId);

        var req = Assert.Single(handler.Requests);
        Assert.Equal("wzbm_live_testkey_1234", req.Headers["X-API-Key"].Single());
        Assert.Equal("Bearer jwt_token_value", req.Headers["Authorization"].Single());
        Assert.Equal("custom-agent/2.0", req.Headers["User-Agent"].Single());
        Assert.Equal("idem-123", req.Headers["Idempotency-Key"].Single());
        Assert.Equal("overridden", req.Headers["X-Default"].Single());
        Assert.Equal("abc", req.Headers["X-Trace"].Single());
        Assert.Same(client.Messages, client.Emails);
    }
}
