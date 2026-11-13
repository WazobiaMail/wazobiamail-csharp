using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WazobiaMail.Http;

internal sealed class WazobiaHttpTransport
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string? _apiKey;
    private readonly string? _accessToken;
    private readonly string _userAgent;
    private readonly TimeSpan _timeout;
    private readonly IReadOnlyDictionary<string, string> _defaultHeaders;

    public WazobiaHttpTransport(
        HttpClient httpClient,
        string baseUrl,
        string? apiKey,
        string? accessToken,
        string userAgent,
        TimeSpan timeout,
        IReadOnlyDictionary<string, string> defaultHeaders)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
        _apiKey = apiKey;
        _accessToken = accessToken;
        _userAgent = userAgent;
        _timeout = timeout;
        _defaultHeaders = defaultHeaders;
    }

    public async Task<ApiResponse<T>> SendJsonAsync<T>(
        HttpMethod method,
        string path,
        object? payload = null,
        IReadOnlyDictionary<string, string>? query = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        HttpContent? content = null;
        if (payload is not null)
        {
            var json = JsonSerializer.Serialize(payload, payload.GetType(), JsonOptions);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var request = BuildRequest(method, path, query, options, content);
        var (bytes, _) = await ExecuteAsync(request, cancellationToken).ConfigureAwait(false);

        if (bytes.Length == 0)
        {
            return new ApiResponse<T>();
        }

        var decoded = JsonSerializer.Deserialize<ApiResponse<T>>(bytes, JsonOptions);
        return decoded ?? new ApiResponse<T>();
    }

    public async Task<ApiResponse<T>> SendMultipartAsync<T>(
        string path,
        MultipartFormDataContent content,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var request = BuildRequest(HttpMethod.Post, path, null, options, content);
        var (bytes, _) = await ExecuteAsync(request, cancellationToken).ConfigureAwait(false);

        if (bytes.Length == 0)
        {
            return new ApiResponse<T>();
        }

        var decoded = JsonSerializer.Deserialize<ApiResponse<T>>(bytes, JsonOptions);
        return decoded ?? new ApiResponse<T>();
    }

    public async Task<BinaryDownload> GetBytesAsync(
        string path,
        IReadOnlyDictionary<string, string>? query = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var request = BuildRequest(HttpMethod.Get, path, query, options, null);
        var (bytes, responseHeaders, contentType) = await ExecuteBinaryAsync(request, cancellationToken).ConfigureAwait(false);
        return new BinaryDownload(bytes, contentType, responseHeaders);
    }

    private HttpRequestMessage BuildRequest(
        HttpMethod method,
        string path,
        IReadOnlyDictionary<string, string>? query,
        RequestOptions? options,
        HttpContent? content)
    {
        var uri = BuildUri(path, query);
        var request = new HttpRequestMessage(method, uri)
        {
            Content = content
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);

        foreach (var pair in _defaultHeaders)
        {
            request.Headers.Remove(pair.Key);
            request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
        }

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            request.Headers.Remove("X-API-Key");
            request.Headers.TryAddWithoutValidation("X-API-Key", _apiKey);
        }

        if (!string.IsNullOrWhiteSpace(_accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        if (!string.IsNullOrWhiteSpace(options?.IdempotencyKey))
        {
            request.Headers.Remove("Idempotency-Key");
            request.Headers.TryAddWithoutValidation("Idempotency-Key", options.IdempotencyKey);
        }

        if (options?.Headers is not null)
        {
            foreach (var pair in options.Headers)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    continue;
                }

                request.Headers.Remove(pair.Key);
                request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }
        }

        return request;
    }

    private Uri BuildUri(string path, IReadOnlyDictionary<string, string>? query)
    {
        var cleanPath = path.TrimStart('/');
        var builder = new StringBuilder(_baseUrl).Append('/').Append(cleanPath);

        if (query is not null && query.Count > 0)
        {
            var first = true;
            foreach (var pair in query)
            {
                if (string.IsNullOrEmpty(pair.Value))
                {
                    continue;
                }

                builder.Append(first ? '?' : '&');
                first = false;
                builder.Append(Uri.EscapeDataString(pair.Key));
                builder.Append('=');
                builder.Append(Uri.EscapeDataString(pair.Value));
            }
        }

        return new Uri(builder.ToString(), UriKind.Absolute);
    }

    private async Task<(byte[] Body, Dictionary<string, string[]> Headers)> ExecuteAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var (body, headers, _) = await ExecuteBinaryAsync(request, cancellationToken).ConfigureAwait(false);
        return (body, headers);
    }

    private async Task<(byte[] Body, Dictionary<string, string[]> Headers, string? ContentType)> ExecuteBinaryAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new WazobiaTimeoutException(
                $"The request to {request.RequestUri} timed out after {_timeout.TotalSeconds:0.##}s.",
                _timeout,
                ex);
        }

        using (response)
        {
            var bodyBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            var headers = CollectHeaders(response);
            var contentType = response.Content.Headers.ContentType?.ToString();

            if (!response.IsSuccessStatusCode)
            {
                throw ParseApiException(response.StatusCode, bodyBytes, response.Headers);
            }

            return (bodyBytes, headers, contentType);
        }
    }

    private static Dictionary<string, string[]> CollectHeaders(HttpResponseMessage response)
    {
        var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            result[header.Key] = header.Value.ToArray();
        }

        foreach (var header in response.Content.Headers)
        {
            result[header.Key] = header.Value.ToArray();
        }

        return result;
    }

    internal static WazobiaApiException ParseApiException(
        HttpStatusCode statusCode,
        byte[] bodyBytes,
        HttpResponseHeaders headers)
    {
        var rawBody = bodyBytes.Length > 0 ? Encoding.UTF8.GetString(bodyBytes) : string.Empty;
        string? message = null;
        string? errorCode = null;
        var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (bodyBytes.Length > 0)
        {
            try
            {
                using var doc = JsonDocument.Parse(bodyBytes);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
                    {
                        var candidate = msgProp.GetString()?.Trim();
                        if (!string.IsNullOrEmpty(candidate))
                        {
                            message = candidate;
                        }
                    }

                    if (string.IsNullOrEmpty(message) && root.TryGetProperty("error", out var errProp))
                    {
                        if (errProp.ValueKind == JsonValueKind.String)
                        {
                            message = errProp.GetString()?.Trim();
                        }
                        else if (errProp.ValueKind == JsonValueKind.Object &&
                                 errProp.TryGetProperty("message", out var nestedMsg) &&
                                 nestedMsg.ValueKind == JsonValueKind.String)
                        {
                            message = nestedMsg.GetString()?.Trim();
                        }
                    }

                    if (root.TryGetProperty("code", out var codeProp) && codeProp.ValueKind == JsonValueKind.String)
                    {
                        errorCode = codeProp.GetString()?.Trim();
                    }

                    if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in errorsProp.EnumerateObject())
                        {
                            fieldErrors[prop.Name] = FormatErrorValue(prop.Value);
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore non-JSON error bodies and fall back to status text.
            }
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            message = DefaultStatusText(statusCode);
        }

        return statusCode switch
        {
            HttpStatusCode.Unauthorized => new WazobiaAuthenticationException(message, statusCode, errorCode, fieldErrors, rawBody),
            HttpStatusCode.Forbidden => new WazobiaForbiddenException(message, errorCode, fieldErrors, rawBody),
            HttpStatusCode.NotFound => new WazobiaNotFoundException(message, errorCode, fieldErrors, rawBody),
            HttpStatusCode.UnprocessableEntity => new WazobiaValidationException(message, statusCode, errorCode, fieldErrors, rawBody),
            HttpStatusCode.BadRequest when fieldErrors.Count > 0 => new WazobiaValidationException(message, statusCode, errorCode, fieldErrors, rawBody),
            HttpStatusCode.TooManyRequests => new WazobiaRateLimitException(message, ParseRetryAfter(headers), errorCode, fieldErrors, rawBody),
            _ => new WazobiaApiException(message, statusCode, errorCode, fieldErrors, rawBody)
        };
    }

    private static string FormatErrorValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Array => string.Join("; ", value.EnumerateArray().Select(FormatErrorValue)),
            _ => value.GetRawText()
        };
    }

    private static TimeSpan? ParseRetryAfter(HttpResponseHeaders headers)
    {
        if (headers.RetryAfter is not null)
        {
            if (headers.RetryAfter.Delta.HasValue && headers.RetryAfter.Delta.Value > TimeSpan.Zero)
            {
                return headers.RetryAfter.Delta.Value;
            }

            if (headers.RetryAfter.Date.HasValue)
            {
                var delta = headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
                if (delta > TimeSpan.Zero)
                {
                    return delta;
                }
            }
        }

        if (headers.TryGetValues("Retry-After", out var values))
        {
            var first = values.FirstOrDefault()?.Trim();
            if (int.TryParse(first, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds) && seconds > 0)
            {
                return TimeSpan.FromSeconds(seconds);
            }
        }

        return null;
    }

    private static string DefaultStatusText(HttpStatusCode statusCode) => (int)statusCode switch
    {
        400 => "Bad request",
        401 => "Authentication failed",
        403 => "Permission denied",
        404 => "Resource not found",
        409 => "Resource conflict",
        422 => "Validation failed",
        429 => "Rate limit exceeded",
        >= 500 => "Server error",
        _ => $"HTTP {(int)statusCode}"
    };
}
