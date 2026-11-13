using System.Net;
using Xunit;

namespace WazobiaMail.Tests;

public sealed class ErrorMappingTests
{
    private static WazobiaMailClient CreateClient(MockHttpMessageHandler handler)
    {
        var http = new HttpClient(handler);
        return new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_test_12345678" }, http);
    }

    [Fact]
    public async Task Maps401ToAuthenticationException()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.Unauthorized, """{"message":"Invalid API key","code":"INVALID_KEY"}""");
        using var client = CreateClient(handler);

        var ex = await Assert.ThrowsAsync<WazobiaAuthenticationException>(() => client.Status.GetAsync());
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal("INVALID_KEY", ex.ErrorCode);
        Assert.Equal("Invalid API key", ex.Message);
    }

    [Fact]
    public async Task Maps403ToForbiddenException()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.Forbidden, """{"error":{"message":"Upgrade required"}}""");
        using var client = CreateClient(handler);

        var ex = await Assert.ThrowsAsync<WazobiaForbiddenException>(() => client.Status.GetAsync());
        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.Equal("Upgrade required", ex.Message);
    }

    [Fact]
    public async Task Maps404ToNotFoundException()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.NotFound, """{"error":"Domain not found"}""");
        using var client = CreateClient(handler);

        var ex = await Assert.ThrowsAsync<WazobiaNotFoundException>(() => client.Domains.GetAsync("dom_missing"));
        Assert.Equal(404, ex.StatusCodeNumber);
        Assert.Equal("Domain not found", ex.Message);
    }

    [Fact]
    public async Task Maps422And400WithErrorsToValidationException()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(
            HttpStatusCode.UnprocessableEntity,
            """{"message":"Validation failed","errors":{"subject":"Subject is required","to":["Invalid email"]}}""");
        handler.EnqueueJson(
            HttpStatusCode.BadRequest,
            """{"message":"Bad input","errors":{"domain":"Invalid hostname"}}""");
        handler.EnqueueJson(
            HttpStatusCode.BadRequest,
            """{"message":"Malformed payload"}""");

        using var client = CreateClient(handler);

        var ex422 = await Assert.ThrowsAsync<WazobiaValidationException>(() => client.Status.GetAsync());
        Assert.Equal("Subject is required", ex422.FieldErrors["subject"]);
        Assert.Equal("Invalid email", ex422.FieldErrors["to"]);

        var ex400Validation = await Assert.ThrowsAsync<WazobiaValidationException>(() => client.Status.GetAsync());
        Assert.Equal("Invalid hostname", ex400Validation.FieldErrors["domain"]);

        var ex400Generic = await Assert.ThrowsAsync<WazobiaApiException>(() => client.Status.GetAsync());
        Assert.IsType<WazobiaApiException>(ex400Generic);
        Assert.Equal("Malformed payload", ex400Generic.Message);
    }

    [Fact]
    public async Task Maps429WithRetryAfterHeaderToRateLimitException()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(
            HttpStatusCode.TooManyRequests,
            """{"message":"Slow down"}""",
            headers => headers.TryAddWithoutValidation("Retry-After", "45"));

        using var client = CreateClient(handler);

        var ex = await Assert.ThrowsAsync<WazobiaRateLimitException>(() => client.Status.GetAsync());
        Assert.Equal(TimeSpan.FromSeconds(45), ex.RetryAfter);
        Assert.Equal("Slow down", ex.Message);
    }

    [Fact]
    public async Task Maps500NonJsonBodyWithFallbackMessage()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueBytes(HttpStatusCode.InternalServerError, System.Text.Encoding.UTF8.GetBytes("upstream error"), "text/plain");
        using var client = CreateClient(handler);

        var ex = await Assert.ThrowsAsync<WazobiaApiException>(() => client.Status.GetAsync());
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Equal("Server error", ex.Message);
        Assert.Equal("upstream error", ex.RawBody);
    }

    [Fact]
    public async Task MapsInternalTimeoutToWazobiaTimeoutException_AndPropagatesCallerCancellation()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueHandler((_, _) => throw new TaskCanceledException("HttpClient timeout"));
        handler.EnqueueHandler((_, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(
            new WazobiaMailOptions { ApiKey = "wzbm_test_12345678", Timeout = TimeSpan.FromSeconds(12) },
            http);

        var timeoutEx = await Assert.ThrowsAsync<WazobiaTimeoutException>(() => client.Status.GetAsync());
        Assert.Equal(TimeSpan.FromSeconds(12), timeoutEx.Timeout);

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.Status.GetAsync(cancellationToken: cts.Token));
    }
}
