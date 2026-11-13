using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace WazobiaMail.Tests;

internal sealed record RecordedRequest(
    HttpMethod Method,
    Uri Uri,
    Dictionary<string, string[]> Headers,
    byte[] BodyBytes,
    string? ContentType);

internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>> _responses = new();

    public List<RecordedRequest> Requests { get; } = new();

    public void EnqueueJson(HttpStatusCode statusCode, string json, Action<HttpResponseHeaders>? configureHeaders = null)
    {
        _responses.Enqueue((_, _) =>
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            configureHeaders?.Invoke(response.Headers);
            return Task.FromResult(response);
        });
    }

    public void EnqueueBytes(HttpStatusCode statusCode, byte[] bytes, string contentType = "application/octet-stream", Action<HttpResponseHeaders>? configureHeaders = null)
    {
        _responses.Enqueue((_, _) =>
        {
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            var response = new HttpResponseMessage(statusCode)
            {
                Content = content
            };
            configureHeaders?.Invoke(response.Headers);
            return Task.FromResult(response);
        });
    }

    public void EnqueueHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        _responses.Enqueue(handler);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in request.Headers)
        {
            headers[header.Key] = header.Value.ToArray();
        }

        byte[] bodyBytes = Array.Empty<byte>();
        string? contentType = null;
        if (request.Content is not null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = header.Value.ToArray();
            }

            contentType = request.Content.Headers.ContentType?.ToString();
            bodyBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        Requests.Add(new RecordedRequest(
            request.Method,
            request.RequestUri!,
            headers,
            bodyBytes,
            contentType));

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException($"No mock response queued for {request.Method} {request.RequestUri}.");
        }

        var next = _responses.Dequeue();
        return await next(request, cancellationToken).ConfigureAwait(false);
    }
}
