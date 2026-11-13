using System.Text.Json.Serialization;

namespace WazobiaMail;

/// <summary>
/// Scoped sending API key metadata.
/// </summary>
public sealed class ApiKey
{
    /// <summary>
    /// Gets or sets the API key ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the descriptive key name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the permission scope.
    /// </summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional bound domain ID.
    /// </summary>
    public string? DomainId { get; set; }

    /// <summary>
    /// Gets or sets the optional bound domain name.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Gets or sets the visible key prefix.
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the key was last used.
    /// </summary>
    public string? LastUsedAt { get; set; }

    /// <summary>
    /// Gets or sets the expiration timestamp.
    /// </summary>
    public string? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the revocation timestamp if revoked.
    /// </summary>
    public string? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }
}

/// <summary>
/// Request payload for creating a sending API key.
/// </summary>
public sealed class CreateApiKeyRequest
{
    /// <summary>
    /// Gets or sets the API key name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional scope (for example <c>sending</c> or <c>full</c>).
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Gets or sets an optional domain ID restriction.
    /// </summary>
    public string? DomainId { get; set; }

    /// <summary>
    /// Gets or sets an optional ISO-8601 expiration timestamp.
    /// </summary>
    public string? ExpiresAt { get; set; }
}

/// <summary>
/// Newly created or regenerated API key including its one-time secret value.
/// </summary>
public sealed class CreatedApiKey
{
    /// <summary>
    /// Gets or sets the API key ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the permission scope.
    /// </summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional bound domain ID.
    /// </summary>
    public string? DomainId { get; set; }

    /// <summary>
    /// Gets or sets the visible key prefix.
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the one-time plaintext API key secret.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional expiration timestamp.
    /// </summary>
    public string? ExpiresAt { get; set; }
}

/// <summary>
/// Wrapper envelope for created/regenerated API keys (<c>{"apiKey": {...}}</c>).
/// </summary>
public sealed class ApiKeyEnvelope
{
    /// <summary>
    /// Gets or sets the created API key.
    /// </summary>
    public CreatedApiKey ApiKey { get; set; } = new();
}

/// <summary>
/// Customer SMTP relay configuration.
/// </summary>
public sealed class SmtpRelay
{
    /// <summary>
    /// Gets or sets the relay ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owning account ID.
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// Gets or sets the relay name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP server hostname.
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP port.
    /// </summary>
    public int SmtpPort { get; set; }

    /// <summary>
    /// Gets or sets the SMTP login username.
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP security mode (<c>starttls</c>, <c>tls</c>, <c>none</c>).
    /// </summary>
    public string SmtpSecurity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relay status (<c>active</c> or <c>inactive</c>).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this relay is the default relay.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets whether a password is stored for this relay.
    /// </summary>
    public bool HasPassword { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public string? UpdatedAt { get; set; }
}

/// <summary>
/// Wrapper envelope for a single SMTP relay (<c>{"smtpRelay": {...}}</c>).
/// </summary>
public sealed class SmtpRelayEnvelope
{
    /// <summary>
    /// Gets or sets the SMTP relay.
    /// </summary>
    public SmtpRelay SmtpRelay { get; set; } = new();
}

/// <summary>
/// Wrapper envelope for SMTP relay lists (<c>{"smtpRelays": [...]}</c>).
/// </summary>
public sealed class SmtpRelaysEnvelope
{
    /// <summary>
    /// Gets or sets the SMTP relays.
    /// </summary>
    public IReadOnlyList<SmtpRelay> SmtpRelays { get; set; } = Array.Empty<SmtpRelay>();
}

/// <summary>
/// Request payload for creating an external SMTP relay.
/// Serializes with snake_case field names required by the WazobiaMail API.
/// </summary>
public sealed class CreateSmtpRelayRequest
{
    /// <summary>
    /// Gets or sets the relay display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP host.
    /// </summary>
    [JsonPropertyName("smtp_host")]
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP port.
    /// </summary>
    [JsonPropertyName("smtp_port")]
    public int SmtpPort { get; set; }

    /// <summary>
    /// Gets or sets the SMTP username.
    /// </summary>
    [JsonPropertyName("smtp_username")]
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP password.
    /// </summary>
    [JsonPropertyName("smtp_password")]
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP security mode (<c>starttls</c>, <c>tls</c>, or <c>none</c>).
    /// </summary>
    [JsonPropertyName("smtp_security")]
    public string SmtpSecurity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this relay becomes the default relay.
    /// </summary>
    [JsonPropertyName("is_default")]
    public bool? IsDefault { get; set; }

    /// <summary>
    /// Gets or sets an optional initial status.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>
/// Request payload for partially updating an external SMTP relay.
/// Serializes with snake_case field names required by the WazobiaMail API.
/// </summary>
public sealed class UpdateSmtpRelayRequest
{
    /// <summary>
    /// Gets or sets the updated relay display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the updated SMTP host.
    /// </summary>
    [JsonPropertyName("smtp_host")]
    public string? SmtpHost { get; set; }

    /// <summary>
    /// Gets or sets the updated SMTP port.
    /// </summary>
    [JsonPropertyName("smtp_port")]
    public int? SmtpPort { get; set; }

    /// <summary>
    /// Gets or sets the updated SMTP username.
    /// </summary>
    [JsonPropertyName("smtp_username")]
    public string? SmtpUsername { get; set; }

    /// <summary>
    /// Gets or sets the updated SMTP password.
    /// </summary>
    [JsonPropertyName("smtp_password")]
    public string? SmtpPassword { get; set; }

    /// <summary>
    /// Gets or sets the updated SMTP security mode.
    /// </summary>
    [JsonPropertyName("smtp_security")]
    public string? SmtpSecurity { get; set; }

    /// <summary>
    /// Gets or sets whether this relay is the default relay.
    /// </summary>
    [JsonPropertyName("is_default")]
    public bool? IsDefault { get; set; }

    /// <summary>
    /// Gets or sets the updated status.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>
/// SMTP access connection instructions.
/// </summary>
public sealed class SmtpConnection
{
    /// <summary>
    /// Gets or sets the SMTP server host.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP server port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the TLS/security mode.
    /// </summary>
    public string Security { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets valid SMTP usernames for the account.
    /// </summary>
    public IReadOnlyList<string> Usernames { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Account-level SMTP sending access status.
/// </summary>
public sealed class SmtpAccess
{
    /// <summary>
    /// Gets or sets whether the account is on a paid plan.
    /// </summary>
    public bool Paid { get; set; }

    /// <summary>
    /// Gets or sets whether the account is eligible for SMTP access.
    /// </summary>
    public bool Eligible { get; set; }

    /// <summary>
    /// Gets or sets whether SMTP access is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the detected account persona.
    /// </summary>
    public string? Persona { get; set; }

    /// <summary>
    /// Gets or sets whether SMTP sending is configured.
    /// </summary>
    public bool Configured { get; set; }

    /// <summary>
    /// Gets or sets the SMTP connection parameters when available.
    /// </summary>
    public SmtpConnection? Connection { get; set; }
}
