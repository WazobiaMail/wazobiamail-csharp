using System.Net;
using Xunit;

namespace WazobiaMail.Tests;

public sealed class AuthTests
{
    [Fact]
    public async Task LoginRegisterRefreshAndMe_WorkAsExpected()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"logged in","data":{"accessToken":"jwt_1","refreshToken":"ref_1","tokenType":"Bearer","expiresIn":900}}""");
        handler.EnqueueJson(HttpStatusCode.Created, """{"success":true,"message":"registered","data":{"accountId":42,"email":"new@acme.com"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"refreshed","data":{"accessToken":"jwt_2","refreshToken":"ref_2"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"me","data":{"accountId":42,"email":"new@acme.com","planId":"free","planName":"Free","monthlyQuota":3000}}""");

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(new WazobiaMailOptions { AllowUnauthenticated = true }, http);

        var login = await client.Auth.LoginAsync(new LoginRequest { Email = "new@acme.com", Password = "secret", TotpCode = "123456" });
        Assert.Equal("jwt_1", login.Data.AccessToken);

        var reg = await client.Auth.RegisterAsync(new RegisterRequest { Email = "new@acme.com", Password = "secret", Username = "acme" });
        Assert.Equal(42, reg.Data.GetProperty("accountId").GetInt64());

        var refreshed = await client.Auth.RefreshAsync("ref_1");
        Assert.Equal("jwt_2", refreshed.Data.AccessToken);

        var me = await client.Auth.MeAsync();
        Assert.Equal(42, me.Data.AccountId);
        Assert.Equal(3000, me.Data.MonthlyQuota);
    }
}
