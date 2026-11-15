using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Configuration options for <see cref="WazobiaMailClient"/>.
/// </summary>
public sealed class WazobiaMailOptions
{
    /// <summary>
    /// The default production WazobiaMail API base URL.
    /// </summary>
    public const string DefaultBaseUrl = "https://app.wazobiamail.com/api/v1";

    /// <summary>
    /// The SDK version string included in the default <c>User-Agent</c> header.
    /// </summary>
    public const string SdkVersion = "0.1.1";

    /// <summary>
    /// The default <c>User-Agent</c> header sent by this SDK.
    /// </summary>
    public const string DefaultUserAgent = $"wazobiamail-csharp/{SdkVersion}";

    /// <summary>
    /// The default HTTP request timeout (30 seconds).
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the WazobiaMail API key sent via the <c>X-API-Key</c> header.
    /// Falls back to <c>WAZOBIA_API_KEY</c> and <c>WAZOBIAMAIL_API_KEY</c> environment variables when omitted.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets an account JWT access token sent via <c>Authorization: Bearer &lt;token&gt;</c>.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the API base URL. Defaults to <see cref="DefaultBaseUrl"/>.
    /// </summary>
    public string BaseUrl { get; set; } = DefaultBaseUrl;

    /// <summary>
    /// Gets or sets the HTTP request timeout for SDK-managed <see cref="HttpClient"/> instances. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = DefaultTimeout;

    /// <summary>
    /// Gets or sets an optional custom <c>User-Agent</c> header. Defaults to <see cref="DefaultUserAgent"/>.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets custom headers appended to every outgoing API request.
    /// </summary>
    public IDictionary<string, string> DefaultHeaders { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets a value indicating whether the client may be constructed without credentials
    /// (useful for public endpoints such as <c>/plans</c>, <c>/status</c>, <c>/auth/login</c>, or <c>/payments/currency</c>).
    /// </summary>
    public bool AllowUnauthenticated { get; set; }

    /// <summary>
    /// Returns a log-safe string representation with credentials masked.
    /// </summary>
    public override string ToString()
    {
        return $"WazobiaMailOptions {{ BaseUrl = {BaseUrl}, ApiKey = {SecretMasker.Mask(ApiKey)}, AccessToken = {SecretMasker.Mask(AccessToken)} }}";
    }
}

/// <summary>
/// Per-request options for customizing headers and idempotency keys.
/// </summary>
public sealed class RequestOptions
{
    /// <summary>
    /// Gets or sets an optional <c>Idempotency-Key</c> header for mutating requests.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Gets or sets per-request HTTP headers that override default client headers.
    /// </summary>
    public IDictionary<string, string>? Headers { get; set; }
}
