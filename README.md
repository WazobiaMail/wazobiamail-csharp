# WazobiaMail .NET / C# SDK

Official, zero-runtime-dependency .NET 8 client library for the [WazobiaMail](https://wazobiamail.com) API (`https://app.wazobiamail.com/api/v1`).

WazobiaMail is a **Resend alternative** for transactional email through an **API or SMTP**, with sending domains, hosted mailboxes, enterprise headless inboxes, and deliverability tools. Start on the **free plan with 3,000 emails/month**.

## Features

- **Zero third-party runtime dependencies** — built solely on `System.Net.Http` and `System.Text.Json` (.NET 8.0+).
- **Thread-safe & DI-ready** — works standalone or with ASP.NET Core `IHttpClientFactory`.
- **Full API coverage** — transactional messages, multipart attachments, attachment uploads, spam checks, tracking, email logs, sending domains, Cloudflare DNS sync, hosted mailboxes, headless inbox reading, API keys, SMTP relays/access, billing, units, plans, status, support tickets, templates, and auth.
- **Typed exceptions** — maps HTTP `401`, `403`, `404`, `422`/`400`, `429` (`RetryAfter`), and timeouts to dedicated exception classes.
- **Offline webhook verification** — constant-time HMAC-SHA256 verification (`X-Wazobia-Signature`).

## Installation

```bash
dotnet add package WazobiaMail
```

## Quick Start

```csharp
using WazobiaMail;

// Uses WAZOBIA_API_KEY or WAZOBIAMAIL_API_KEY environment variable when omitted.
using var client = new WazobiaMailClient("wzbm_live_your_api_key");

var response = await client.SendAsync(new SendMessageRequest
{
    From = "Acme <hello@mail.acme.com>",
    To = ["customer@example.com"],
    Subject = "Your order #1001",
    Html = "<h1>Thank you for your order!</h1>",
    Text = "Thank you for your order!",
    IdempotencyKey = "order-1001-confirmation"
});

Console.WriteLine($"Tracking ID: {response.Data.Result.TrackingId}");
```

## ASP.NET Core Dependency Injection

```csharp
builder.Services.AddSingleton(new WazobiaMailOptions
{
    ApiKey = builder.Configuration["WazobiaMail:ApiKey"]
});

builder.Services.AddHttpClient<WazobiaMailClient>();
```

Then inject `WazobiaMailClient` into controllers, minimal APIs, or background workers. When constructed with an injected `HttpClient`, `WazobiaMailClient.Dispose()` leaves the `HttpClient` lifecycle to `IHttpClientFactory`.

## Sending Attachments

### Direct Multipart Upload

```csharp
var pdf = await FileUpload.FromFileAsync("invoice.pdf", "application/pdf");

var response = await client.Messages.SendWithAttachmentsAsync(
    new SendMessageRequest
    {
        From = "Billing <billing@acme.com>",
        To = ["customer@example.com"],
        Subject = "Your Invoice",
        Text = "Please find your invoice attached."
    },
    [pdf]);
```

### Pre-upload & URL Attachment

```csharp
var upload = await client.Messages.UploadAttachmentAsync(pdf);

await client.SendAsync(new SendMessageRequest
{
    From = "Billing <billing@acme.com>",
    To = ["customer@example.com"],
    Subject = "Your Invoice",
    Html = "<p>Attached.</p>",
    Attachments = [new UrlAttachment(upload.Data.File.PublicUrl!, "invoice.pdf")]
});
```

## Domains, Mailboxes & Headless Inboxes

```csharp
// Add and verify a sending domain
var domain = await client.Domains.CreateAsync(new CreateDomainRequest
{
    Domain = "acme.com",
    MailMode = "hosted"
});
var dns = await client.Domains.GetDnsRecordsAsync(domain.Data.Domain.Id);
var verification = await client.Domains.VerifyAsync(domain.Data.Domain.Id);

// Provision a hosted mailbox and read its headless inbox
var mailbox = await client.Mailboxes.CreateAsync(new CreateMailboxRequest
{
    DomainId = domain.Data.Domain.Id,
    LocalPart = "support"
});

var inbox = await client.Mailboxes.ListMessagesAsync(
    mailbox.Data.Mailbox.Id,
    new ListMessagesParams { UnreadOnly = true, Limit = 20 });
```

## Error Handling

```csharp
try
{
    await client.SendAsync(request);
}
catch (WazobiaValidationException ex)
{
    foreach (var (field, error) in ex.FieldErrors)
    {
        Console.WriteLine($"{field}: {error}");
    }
}
catch (WazobiaRateLimitException ex)
{
    Console.WriteLine($"Rate limited. Retry after: {ex.RetryAfter}");
}
catch (WazobiaApiException ex)
{
    Console.WriteLine($"API error {(int)ex.StatusCode}: {ex.Message}");
}
```

## Webhook Signature Verification

```csharp
// Minimal API / ASP.NET Core webhook endpoint
app.MapPost("/webhooks/wazobiamail", async (HttpRequest req, WazobiaMailClient client, IConfiguration config) =>
{
    using var reader = new StreamReader(req.Body);
    var rawBody = await reader.ReadToEndAsync();
    var signature = req.Headers[WebhooksHelper.SignatureHeader].ToString();
    var secret = config["WazobiaMail:WebhookSecret"];

    try
    {
        var evt = client.Webhooks.ConstructEvent(rawBody, signature, secret);
        return Results.Ok();
    }
    catch (WazobiaWebhookException)
    {
        return Results.Unauthorized();
    }
});
```

## Local Build, Test & Pack

```bash
dotnet build -c Release
dotnet test -c Release
dotnet pack src/WazobiaMail/WazobiaMail.csproj -c Release -o artifacts
```

## License

MIT — see [LICENSE](LICENSE).
