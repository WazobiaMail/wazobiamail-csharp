using System.Net;
using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Thread-safe client for the WazobiaMail REST API.
/// </summary>
public sealed class WazobiaMailClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly string? _apiKey;
    private readonly string? _accessToken;

    /// <summary>
    /// Initializes a new <see cref="WazobiaMailClient"/> using an API key or environment fallback
    /// (<c>WAZOBIA_API_KEY</c> then <c>WAZOBIAMAIL_API_KEY</c>).
    /// </summary>
    /// <param name="apiKey">Optional API key. If null or whitespace, environment variables are checked.</param>
    public WazobiaMailClient(string? apiKey = null)
        : this(new WazobiaMailOptions { ApiKey = apiKey }, httpClient: null)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="WazobiaMailClient"/> with explicit options and an optional caller-managed <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="options">Client configuration options.</param>
    /// <param name="httpClient">Optional <see cref="HttpClient"/> (such as one created by <c>IHttpClientFactory</c>). When provided, it will not be disposed by this client.</param>
    public WazobiaMailClient(WazobiaMailOptions options, HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        var rawBaseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
            ? WazobiaMailOptions.DefaultBaseUrl
            : options.BaseUrl.Trim();

        if (!rawBaseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !rawBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("BaseUrl must start with http:// or https://.", nameof(options));
        }

        if (options.Timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Timeout must be greater than zero.");
        }

        BaseUrl = rawBaseUrl.TrimEnd('/');

        _apiKey = string.IsNullOrWhiteSpace(options.ApiKey)
            ? FirstEnvironmentVariable("WAZOBIA_API_KEY", "WAZOBIAMAIL_API_KEY")
            : options.ApiKey.Trim();

        _accessToken = string.IsNullOrWhiteSpace(options.AccessToken)
            ? null
            : options.AccessToken.Trim();

        if (string.IsNullOrEmpty(_apiKey) &&
            string.IsNullOrEmpty(_accessToken) &&
            !options.AllowUnauthenticated)
        {
            throw new WazobiaAuthenticationException(
                "A WazobiaMail API key or access token is required. Pass it to WazobiaMailClient, set WAZOBIA_API_KEY / WAZOBIAMAIL_API_KEY, or set AllowUnauthenticated = true.",
                HttpStatusCode.Unauthorized);
        }

        var userAgent = string.IsNullOrWhiteSpace(options.UserAgent)
            ? WazobiaMailOptions.DefaultUserAgent
            : options.UserAgent.Trim();

        if (httpClient is null)
        {
            _httpClient = new HttpClient { Timeout = options.Timeout };
            _ownsHttpClient = true;
        }
        else
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }

        var defaultHeaders = new Dictionary<string, string>(options.DefaultHeaders, StringComparer.OrdinalIgnoreCase);

        var transport = new WazobiaHttpTransport(
            _httpClient,
            BaseUrl,
            _apiKey,
            _accessToken,
            userAgent,
            options.Timeout,
            defaultHeaders);

        Messages = new MessagesResource(transport);
        Emails = Messages;
        Domains = new DomainsResource(transport);
        Mailboxes = new MailboxesResource(transport);
        ApiKeys = new ApiKeysResource(transport);
        Smtp = new SmtpResource(transport);
        Billing = new BillingResource(transport);
        Units = new UnitsResource(transport);
        Plans = new PlansResource(transport);
        Status = new StatusResource(transport);
        Support = new SupportResource(transport);
        Templates = new TemplatesResource(transport);
        Auth = new AuthResource(transport);
        Webhooks = new WebhooksHelper();
    }

    /// <summary>
    /// Initializes a new <see cref="WazobiaMailClient"/> using a DI-managed <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> instance provided by DI or <c>IHttpClientFactory</c>.</param>
    /// <param name="options">Optional client options. Defaults to environment variable resolution when null.</param>
    public WazobiaMailClient(HttpClient httpClient, WazobiaMailOptions? options = null)
        : this(options ?? new WazobiaMailOptions(), httpClient ?? throw new ArgumentNullException(nameof(httpClient)))
    {
    }

    /// <summary>
    /// Gets the resolved API base URL without a trailing slash.
    /// </summary>
    public string BaseUrl { get; }

    /// <summary>
    /// Gets the transactional messages, attachments, spam check, tracking, and logs resource.
    /// </summary>
    public MessagesResource Messages { get; }

    /// <summary>
    /// Ergonomic alias for <see cref="Messages"/>.
    /// </summary>
    public MessagesResource Emails { get; }

    /// <summary>
    /// Gets the sending domains, DNS verification, and Cloudflare DNS integration resource.
    /// </summary>
    public DomainsResource Domains { get; }

    /// <summary>
    /// Gets the hosted mailboxes, credentials, storage quotas, and headless inbox resource.
    /// </summary>
    public MailboxesResource Mailboxes { get; }

    /// <summary>
    /// Gets the sending API keys resource.
    /// </summary>
    public ApiKeysResource ApiKeys { get; }

    /// <summary>
    /// Gets the SMTP relay and SMTP access resource.
    /// </summary>
    public SmtpResource Smtp { get; }

    /// <summary>
    /// Gets the billing invoices, subscriptions, and currency detection resource.
    /// </summary>
    public BillingResource Billing { get; }

    /// <summary>
    /// Gets the email credit units resource.
    /// </summary>
    public UnitsResource Units { get; }

    /// <summary>
    /// Gets the subscription plans and effective limits resource.
    /// </summary>
    public PlansResource Plans { get; }

    /// <summary>
    /// Gets the account status resource.
    /// </summary>
    public StatusResource Status { get; }

    /// <summary>
    /// Gets the customer support tickets resource.
    /// </summary>
    public SupportResource Support { get; }

    /// <summary>
    /// Gets the templates resource.
    /// </summary>
    public TemplatesResource Templates { get; }

    /// <summary>
    /// Gets the account authentication resource.
    /// </summary>
    public AuthResource Auth { get; }

    /// <summary>
    /// Gets the offline webhook HMAC-SHA256 verification and parsing helper.
    /// </summary>
    public WebhooksHelper Webhooks { get; }

    /// <summary>
    /// Convenience shortcut for <see cref="MessagesResource.SendAsync"/>.
    /// </summary>
    public Task<ApiResponse<SendEnvelope>> SendAsync(
        SendMessageRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Messages.SendAsync(request, options, cancellationToken);
    }

    /// <summary>
    /// Returns a log-safe string representation with credentials masked.
    /// </summary>
    public override string ToString()
    {
        return $"WazobiaMailClient {{ BaseUrl = {BaseUrl}, ApiKey = {SecretMasker.Mask(_apiKey)}, AccessToken = {SecretMasker.Mask(_accessToken)} }}";
    }

    /// <summary>
    /// Disposes the internally created <see cref="HttpClient"/> if one was not injected by the caller.
    /// </summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private static string? FirstEnvironmentVariable(params string[] names)
    {
        foreach (var name in names)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }
}
