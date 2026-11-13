namespace WazobiaMail;

/// <summary>
/// Request payload for authenticating with email and password.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// Gets or sets the account email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional 6-digit TOTP MFA code.
    /// </summary>
    public string? TotpCode { get; set; }
}

/// <summary>
/// Request payload for registering a new WazobiaMail account.
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>
    /// Gets or sets the account email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets an optional initial plan ID.
    /// </summary>
    public string? PlanId { get; set; }
}

/// <summary>
/// Request payload for refreshing an expired JWT access token.
/// </summary>
public sealed class RefreshTokenRequest
{
    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Token pair and optional account metadata returned by <c>/auth/login</c> and <c>/auth/refresh</c>.
/// </summary>
public sealed class AuthPayload
{
    /// <summary>
    /// Gets or sets the short-lived JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the token type (typically <c>Bearer</c>).
    /// </summary>
    public string? TokenType { get; set; }

    /// <summary>
    /// Gets or sets the access token lifetime in seconds.
    /// </summary>
    public long? ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the authenticated account details when included.
    /// </summary>
    public CurrentAccount? Account { get; set; }
}

/// <summary>
/// Authenticated account profile returned by <c>GET /auth/me</c>.
/// </summary>
public sealed class CurrentAccount
{
    /// <summary>
    /// Gets or sets the numeric account ID.
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// Gets or sets the account email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the active plan ID.
    /// </summary>
    public string PlanId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected plan ID.
    /// </summary>
    public string SelectedPlanId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account role.
    /// </summary>
    public string SaasRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authentication provider.
    /// </summary>
    public string AuthProvider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets emails sent in the current hour.
    /// </summary>
    public int HourlySent { get; set; }

    /// <summary>
    /// Gets or sets emails sent today.
    /// </summary>
    public int DailySent { get; set; }

    /// <summary>
    /// Gets or sets emails sent this month.
    /// </summary>
    public int MonthlySent { get; set; }

    /// <summary>
    /// Gets or sets extra email quota credits.
    /// </summary>
    public long ExtraEmailQuota { get; set; }

    /// <summary>
    /// Gets or sets whether sending is locked on the account.
    /// </summary>
    public bool SendingLocked { get; set; }

    /// <summary>
    /// Gets or sets the hourly sending quota.
    /// </summary>
    public int HourlyQuota { get; set; }

    /// <summary>
    /// Gets or sets the daily sending quota.
    /// </summary>
    public int DailyQuota { get; set; }

    /// <summary>
    /// Gets or sets the monthly sending quota.
    /// </summary>
    public int MonthlyQuota { get; set; }

    /// <summary>
    /// Gets or sets the human-readable plan name.
    /// </summary>
    public string PlanName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }
}
