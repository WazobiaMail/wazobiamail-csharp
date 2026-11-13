using System.Net;

namespace WazobiaMail;

/// <summary>
/// Base exception for all errors thrown by the WazobiaMail SDK.
/// </summary>
public class WazobiaException : Exception
{
    /// <summary>
    /// Initializes a new <see cref="WazobiaException"/> instance.
    /// </summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="innerException">An optional inner exception that caused this error.</param>
    public WazobiaException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when the WazobiaMail API returns a non-2xx HTTP response.
/// </summary>
public class WazobiaApiException : WazobiaException
{
    /// <summary>
    /// Gets the HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the numeric HTTP status code returned by the API.
    /// </summary>
    public int StatusCodeNumber => (int)StatusCode;

    /// <summary>
    /// Gets the optional machine-readable error code returned by the API.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets any per-field validation errors returned by the API.
    /// </summary>
    public IReadOnlyDictionary<string, string> FieldErrors { get; }

    /// <summary>
    /// Gets the raw response body returned by the API.
    /// </summary>
    public string RawBody { get; }

    /// <summary>
    /// Initializes a new <see cref="WazobiaApiException"/> instance.
    /// </summary>
    public WazobiaApiException(
        string message,
        HttpStatusCode statusCode,
        string? errorCode = null,
        IReadOnlyDictionary<string, string>? fieldErrors = null,
        string? rawBody = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        FieldErrors = fieldErrors ?? new Dictionary<string, string>();
        RawBody = rawBody ?? string.Empty;
    }
}

/// <summary>
/// Exception thrown when authentication fails (HTTP 401) or required credentials are missing.
/// </summary>
public sealed class WazobiaAuthenticationException : WazobiaApiException
{
    /// <summary>
    /// Initializes a new <see cref="WazobiaAuthenticationException"/> instance.
    /// </summary>
    public WazobiaAuthenticationException(
        string message = "Authentication failed",
        HttpStatusCode statusCode = HttpStatusCode.Unauthorized,
        string? errorCode = null,
        IReadOnlyDictionary<string, string>? fieldErrors = null,
        string? rawBody = null)
        : base(message, statusCode, errorCode, fieldErrors, rawBody)
    {
    }
}

/// <summary>
/// Exception thrown when access is denied due to permissions or plan limits (HTTP 403).
/// </summary>
public sealed class WazobiaForbiddenException : WazobiaApiException
{
    /// <summary>
    /// Initializes a new <see cref="WazobiaForbiddenException"/> instance.
    /// </summary>
    public WazobiaForbiddenException(
        string message = "Permission denied",
        string? errorCode = null,
        IReadOnlyDictionary<string, string>? fieldErrors = null,
        string? rawBody = null)
        : base(message, HttpStatusCode.Forbidden, errorCode, fieldErrors, rawBody)
    {
    }
}

/// <summary>
/// Exception thrown when a requested resource does not exist (HTTP 404).
/// </summary>
public sealed class WazobiaNotFoundException : WazobiaApiException
{
    /// <summary>
    /// Initializes a new <see cref="WazobiaNotFoundException"/> instance.
    /// </summary>
    public WazobiaNotFoundException(
        string message = "Resource not found",
        string? errorCode = null,
        IReadOnlyDictionary<string, string>? fieldErrors = null,
        string? rawBody = null)
        : base(message, HttpStatusCode.NotFound, errorCode, fieldErrors, rawBody)
    {
    }
}

/// <summary>
/// Exception thrown when the API rejects invalid input (HTTP 422 or HTTP 400 with field errors).
/// </summary>
public sealed class WazobiaValidationException : WazobiaApiException
{
    /// <summary>
    /// Initializes a new <see cref="WazobiaValidationException"/> instance.
    /// </summary>
    public WazobiaValidationException(
        string message = "Validation failed",
        HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity,
        string? errorCode = null,
        IReadOnlyDictionary<string, string>? fieldErrors = null,
        string? rawBody = null)
        : base(message, statusCode, errorCode, fieldErrors, rawBody)
    {
    }
}

/// <summary>
/// Exception thrown when the API rate limit has been exceeded (HTTP 429).
/// </summary>
public sealed class WazobiaRateLimitException : WazobiaApiException
{
    /// <summary>
    /// Gets the suggested delay before retrying, parsed from the <c>Retry-After</c> header if provided.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>
    /// Initializes a new <see cref="WazobiaRateLimitException"/> instance.
    /// </summary>
    public WazobiaRateLimitException(
        string message = "Rate limit exceeded",
        TimeSpan? retryAfter = null,
        string? errorCode = null,
        IReadOnlyDictionary<string, string>? fieldErrors = null,
        string? rawBody = null)
        : base(message, HttpStatusCode.TooManyRequests, errorCode, fieldErrors, rawBody)
    {
        RetryAfter = retryAfter;
    }
}

/// <summary>
/// Exception thrown when an HTTP request exceeds the configured timeout.
/// </summary>
public sealed class WazobiaTimeoutException : WazobiaException
{
    /// <summary>
    /// Gets the configured timeout duration.
    /// </summary>
    public TimeSpan Timeout { get; }

    /// <summary>
    /// Initializes a new <see cref="WazobiaTimeoutException"/> instance.
    /// </summary>
    public WazobiaTimeoutException(string message, TimeSpan timeout, Exception? innerException = null)
        : base(message, innerException)
    {
        Timeout = timeout;
    }
}

/// <summary>
/// Exception thrown when webhook signature verification or payload parsing fails.
/// </summary>
public sealed class WazobiaWebhookException : WazobiaException
{
    /// <summary>
    /// Initializes a new <see cref="WazobiaWebhookException"/> instance.
    /// </summary>
    public WazobiaWebhookException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
