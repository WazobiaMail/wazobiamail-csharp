namespace WazobiaMail;

/// <summary>
/// Sending domain entity managed by WazobiaMail.
/// </summary>
public sealed class Domain
{
    /// <summary>
    /// Gets or sets the domain ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owning account ID.
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// Gets or sets the active sending domain name.
    /// </summary>
    public string DomainName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the domain name as returned in JSON (<c>domain</c>).
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("domain")]
    public string DomainValue
    {
        get => DomainName;
        set => DomainName = value;
    }

    /// <summary>
    /// Gets or sets the originally requested domain name when it differs from <see cref="DomainName"/>.
    /// </summary>
    public string? RequestedDomain { get; set; }

    /// <summary>
    /// Gets or sets the verification status (<c>pending</c>, <c>verified</c>, <c>failed</c>).
    /// </summary>
    public string VerificationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the domain is verified.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsVerified =>
        string.Equals(VerificationStatus, "verified", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets a value indicating whether this is the default sending domain.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets the domain role (<c>primary</c> or <c>sending</c>).
    /// </summary>
    public string DomainRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent domain ID if this is a sending subdomain.
    /// </summary>
    public string? ParentDomainId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether default service inboxes were provisioned.
    /// </summary>
    public bool CreateDefaultInboxes { get; set; }

    /// <summary>
    /// Gets or sets the mail mode (<c>hosted</c> or <c>sending_only</c>).
    /// </summary>
    public string MailMode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }
}

/// <summary>
/// Wrapper envelope for single domain responses (<c>{"domain": {...}}</c>).
/// </summary>
public sealed class DomainEnvelope
{
    /// <summary>
    /// Gets or sets the domain record.
    /// </summary>
    public Domain Domain { get; set; } = new();
}

/// <summary>
/// Result returned when deleting a sending domain.
/// </summary>
public sealed class DomainDeleted
{
    /// <summary>
    /// Gets or sets the deleted domain ID.
    /// </summary>
    public string DomainId { get; set; } = string.Empty;
}

/// <summary>
/// Request payload for registering a new sending domain.
/// </summary>
public sealed class CreateDomainRequest
{
    /// <summary>
    /// Gets or sets the domain name to register.
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this domain becomes the account default.
    /// </summary>
    public bool? IsDefault { get; set; }

    /// <summary>
    /// Gets or sets whether to send from the apex domain instead of a dedicated subdomain.
    /// </summary>
    public bool? UseRootDomain { get; set; }

    /// <summary>
    /// Gets or sets whether mailboxes are hosted on the main domain.
    /// </summary>
    public bool? HostInboxesOnMainDomain { get; set; }

    /// <summary>
    /// Gets or sets an optional custom sending subdomain label (such as <c>mta</c>).
    /// </summary>
    public string? SendingSubdomain { get; set; }

    /// <summary>
    /// Gets or sets the mail mode (<c>hosted</c> or <c>sending_only</c>).
    /// </summary>
    public string? MailMode { get; set; }
}

/// <summary>
/// Request payload for updating a sending domain.
/// </summary>
public sealed class UpdateDomainRequest
{
    /// <summary>
    /// Gets or sets the updated domain name.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Gets or sets whether this domain is the default sending domain.
    /// </summary>
    public bool? IsDefault { get; set; }

    /// <summary>
    /// Gets or sets whether to send from the root domain.
    /// </summary>
    public bool? UseRootDomain { get; set; }

    /// <summary>
    /// Gets or sets the mail mode (<c>hosted</c> or <c>sending_only</c>).
    /// </summary>
    public string? MailMode { get; set; }
}

/// <summary>
/// Required DNS record for domain verification.
/// </summary>
public sealed class DnsRecord
{
    /// <summary>
    /// Gets or sets the record ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the DNS record type (TXT, MX, CNAME, etc.).
    /// </summary>
    public string RecordType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the DNS host name.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected DNS record value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recommended TTL in seconds.
    /// </summary>
    public int Ttl { get; set; }

    /// <summary>
    /// Gets or sets the current verification status of this record.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this record was last checked.
    /// </summary>
    public string? LastCheckedAt { get; set; }
}

/// <summary>
/// Domain DNS instructions and record list.
/// </summary>
public sealed class DomainDns
{
    /// <summary>
    /// Gets or sets a value indicating whether DNS provisioning is still in progress.
    /// </summary>
    public bool Provisioning { get; set; }

    /// <summary>
    /// Gets or sets the domain ID.
    /// </summary>
    public string DomainId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the domain name.
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overall verification status.
    /// </summary>
    public string VerificationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the required DNS records.
    /// </summary>
    public IReadOnlyList<DnsRecord> Records { get; set; } = Array.Empty<DnsRecord>();

    /// <summary>
    /// Gets or sets a zone-file style copy block of records.
    /// </summary>
    public string CopyBlock { get; set; } = string.Empty;
}

/// <summary>
/// Wrapper envelope for DNS record inspection (<c>{"domain": {...}}</c>).
/// </summary>
public sealed class DomainDnsEnvelope
{
    /// <summary>
    /// Gets or sets the domain DNS details.
    /// </summary>
    public DomainDns Domain { get; set; } = new();
}

/// <summary>
/// Single DNS record verification result.
/// </summary>
public sealed class VerificationCheck
{
    /// <summary>
    /// Gets or sets the DNS record type.
    /// </summary>
    public string RecordType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the DNS host.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected DNS value.
    /// </summary>
    public string ExpectedValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the observed DNS matched.
    /// </summary>
    public bool Matched { get; set; }

    /// <summary>
    /// Gets or sets the DNS values observed during lookup.
    /// </summary>
    public IReadOnlyList<string> ObservedValues { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Result of verifying or refreshing domain DNS records.
/// </summary>
public sealed class DomainVerification
{
    /// <summary>
    /// Gets or sets the domain ID.
    /// </summary>
    public string DomainId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the domain name.
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated verification status.
    /// </summary>
    public string VerificationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the count of verified records.
    /// </summary>
    public int VerifiedCount { get; set; }

    /// <summary>
    /// Gets or sets the count of pending records.
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// Gets or sets individual verification check results.
    /// </summary>
    public IReadOnlyList<VerificationCheck> Checks { get; set; } = Array.Empty<VerificationCheck>();
}

/// <summary>
/// Wrapper envelope for verification responses (<c>{"verification": {...}}</c>).
/// </summary>
public sealed class VerificationEnvelope
{
    /// <summary>
    /// Gets or sets the domain verification report.
    /// </summary>
    public DomainVerification Verification { get; set; } = new();
}

/// <summary>
/// Cloudflare DNS integration status.
/// </summary>
public sealed class CloudflareStatus
{
    /// <summary>
    /// Gets or sets whether Cloudflare integration is configured on the platform.
    /// </summary>
    public bool Configured { get; set; }

    /// <summary>
    /// Gets or sets whether the account is eligible for managed DNS.
    /// </summary>
    public bool ManagedEligible { get; set; }

    /// <summary>
    /// Gets or sets whether OAuth is configured.
    /// </summary>
    public bool OauthConfigured { get; set; }

    /// <summary>
    /// Gets or sets whether Domain Connect is available.
    /// </summary>
    public bool DomainConnectAvailable { get; set; }

    /// <summary>
    /// Gets or sets the connection source (<c>customerOAuth</c> or <c>platformToken</c>).
    /// </summary>
    public string? ConnectionSource { get; set; }

    /// <summary>
    /// Gets or sets whether Cloudflare is currently connected.
    /// </summary>
    public bool Connected { get; set; }

    /// <summary>
    /// Gets or sets whether the Cloudflare OAuth token requires reconnection.
    /// </summary>
    public bool NeedsReconnect { get; set; }

    /// <summary>
    /// Gets or sets the connected Cloudflare account email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the granted Cloudflare OAuth scopes.
    /// </summary>
    public string Scopes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the matched Cloudflare zone ID when scoped to a domain.
    /// </summary>
    public string? ZoneId { get; set; }

    /// <summary>
    /// Gets or sets the matched domain name when scoped to a domain.
    /// </summary>
    public string? Domain { get; set; }
}

/// <summary>
/// Cloudflare OAuth authorization URL payload.
/// </summary>
public sealed class CloudflareConnect
{
    /// <summary>
    /// Gets or sets the OAuth authorization URL to open in a browser.
    /// </summary>
    public string AuthorizationUrl { get; set; } = string.Empty;
}

internal sealed class CloudflareSyncRequest
{
    public string DomainId { get; set; } = string.Empty;
}

/// <summary>
/// Result of publishing DNS records to a connected Cloudflare zone.
/// </summary>
public sealed class CloudflareSyncResult
{
    /// <summary>
    /// Gets or sets the Cloudflare zone name.
    /// </summary>
    public string Zone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of records added.
    /// </summary>
    public int Added { get; set; }

    /// <summary>
    /// Gets or sets the number of records updated.
    /// </summary>
    public int Updated { get; set; }

    /// <summary>
    /// Gets or sets the total number of records synchronized.
    /// </summary>
    public int Records { get; set; }

    /// <summary>
    /// Gets or sets the resulting domain verification status.
    /// </summary>
    public string VerificationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether receiving MX records are ready.
    /// </summary>
    public bool? ReceivingReady { get; set; }

    /// <summary>
    /// Gets or sets whether DMARC is ready.
    /// </summary>
    public bool? DmarcReady { get; set; }

    /// <summary>
    /// Gets or sets the verified record count.
    /// </summary>
    public int? VerifiedCount { get; set; }

    /// <summary>
    /// Gets or sets the pending record count.
    /// </summary>
    public int? PendingCount { get; set; }

    /// <summary>
    /// Gets or sets an optional informational note.
    /// </summary>
    public string? Note { get; set; }
}
