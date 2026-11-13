using System.Text.Json;
using System.Text.Json.Serialization;

namespace WazobiaMail;

/// <summary>
/// Billing invoice record.
/// </summary>
public sealed class Invoice
{
    /// <summary>
    /// Gets or sets the invoice ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owning account ID.
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// Gets or sets the invoice amount.
    /// </summary>
    public double Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code (<c>NGN</c> or <c>USD</c>).
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the billing purpose (<c>subscription</c>, <c>email_units</c>, etc.).
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional structured invoice metadata.
    /// </summary>
    public JsonElement? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the due date.
    /// </summary>
    public string? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the payment timestamp.
    /// </summary>
    public string? PaidAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }
}

/// <summary>
/// Wrapper envelope for a single invoice (<c>{"invoice": {...}}</c>).
/// </summary>
public sealed class InvoiceEnvelope
{
    /// <summary>
    /// Gets or sets the invoice record.
    /// </summary>
    public Invoice Invoice { get; set; } = new();
}

/// <summary>
/// Plan summary embedded inside <see cref="Subscription"/>.
/// </summary>
public sealed class SubscriptionPlanSummary
{
    /// <summary>
    /// Gets or sets the plan ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plan name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the monthly USD price.
    /// </summary>
    public double UsdPrice { get; set; }

    /// <summary>
    /// Gets or sets the monthly NGN price.
    /// </summary>
    public double NairaPrice { get; set; }

    /// <summary>
    /// Gets or sets the monthly email quota.
    /// </summary>
    public long MonthlyQuota { get; set; }
}

/// <summary>
/// Active account subscription details.
/// </summary>
public sealed class Subscription
{
    /// <summary>
    /// Gets or sets the subscription ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account ID.
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// Gets or sets the plan ID.
    /// </summary>
    public string PlanId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscription status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscription start timestamp.
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the subscription end timestamp.
    /// </summary>
    public string? EndDate { get; set; }

    /// <summary>
    /// Gets or sets whether automatic renewal is enabled.
    /// </summary>
    public bool AutoRenew { get; set; }

    /// <summary>
    /// Gets or sets the billing interval (<c>monthly</c> or <c>yearly</c>).
    /// </summary>
    public string BillingInterval { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the embedded plan summary.
    /// </summary>
    public SubscriptionPlanSummary? Plan { get; set; }
}

/// <summary>
/// Wrapper envelope for the current subscription (<c>{"subscription": {...} | null}</c>).
/// </summary>
public sealed class SubscriptionEnvelope
{
    /// <summary>
    /// Gets or sets the active subscription, or <see langword="null"/> if none is active.
    /// </summary>
    public Subscription? Subscription { get; set; }
}

/// <summary>
/// Detected billing country and currency returned by <c>/payments/currency</c>.
/// </summary>
public sealed class DetectedCurrency
{
    /// <summary>
    /// Gets or sets the two-letter ISO country code if detected.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the resolved billing currency (<c>NGN</c> or <c>USD</c>).
    /// </summary>
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// Email credit/unit balance summary.
/// </summary>
public sealed class UnitsSummary
{
    /// <summary>
    /// Gets or sets the monthly plan allowance.
    /// </summary>
    public long IncludedAllowance { get; set; }

    /// <summary>
    /// Gets or sets the included allowance consumed this period.
    /// </summary>
    public long IncludedUsed { get; set; }

    /// <summary>
    /// Gets or sets the remaining included allowance.
    /// </summary>
    public long IncludedRemaining { get; set; }

    /// <summary>
    /// Gets or sets the non-expiring purchased email credit balance.
    /// </summary>
    public long PurchasedBalance { get; set; }

    /// <summary>
    /// Gets or sets whether the plan has unlimited sending.
    /// </summary>
    public bool Unlimited { get; set; }

    /// <summary>
    /// Gets or sets whether the account can purchase credit packs.
    /// </summary>
    public bool CanBuyCredits { get; set; }
}

/// <summary>
/// Purchasable email credit pack.
/// </summary>
public sealed class UnitPack
{
    /// <summary>
    /// Gets or sets the pack ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pack name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pack description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of email credits included.
    /// </summary>
    public long Units { get; set; }

    /// <summary>
    /// Gets or sets the NGN price in minor units (kobo).
    /// </summary>
    public long PriceNgnMinor { get; set; }

    /// <summary>
    /// Gets or sets the USD price in minor units (cents).
    /// </summary>
    public long PriceUsdMinor { get; set; }

    /// <summary>
    /// Gets or sets whether the pack is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the display sort order.
    /// </summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// Supported purchase currencies for WazobiaMail email units.
/// </summary>
[JsonConverter(typeof(UnitCurrencyJsonConverter))]
public enum UnitCurrency
{
    /// <summary>
    /// Nigerian Naira (<c>NGN</c>).
    /// </summary>
    Ngn,

    /// <summary>
    /// US Dollar (<c>USD</c>).
    /// </summary>
    Usd
}

internal sealed class UnitCurrencyJsonConverter : JsonConverter<UnitCurrency>
{
    public override UnitCurrency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString();
        return raw?.ToUpperInvariant() switch
        {
            "NGN" => UnitCurrency.Ngn,
            "USD" => UnitCurrency.Usd,
            _ => throw new JsonException($"Unknown currency '{raw}'. Expected 'NGN' or 'USD'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, UnitCurrency value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value == UnitCurrency.Ngn ? "NGN" : "USD");
    }
}

/// <summary>
/// Request payload for purchasing an email credit pack.
/// </summary>
public sealed class PurchaseUnitsRequest
{
    /// <summary>
    /// Gets or sets the credit pack ID.
    /// </summary>
    public string PackId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the checkout currency.
    /// </summary>
    public UnitCurrency Currency { get; set; } = UnitCurrency.Usd;
}

/// <summary>
/// Created invoice/payment details returned when purchasing an email credit pack.
/// </summary>
public sealed class UnitPurchase
{
    /// <summary>
    /// Gets or sets the created invoice ID.
    /// </summary>
    public string InvoiceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the purchased credit quantity.
    /// </summary>
    public long Units { get; set; }

    /// <summary>
    /// Gets or sets the total payable amount in minor units.
    /// </summary>
    public long AmountMinor { get; set; }

    /// <summary>
    /// Gets or sets the credit amount in minor units.
    /// </summary>
    public long CreditAmountMinor { get; set; }

    /// <summary>
    /// Gets or sets the service fee amount in minor units.
    /// </summary>
    public long ServiceAmountMinor { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the purchase status (for example <c>pending_payment</c>).
    /// </summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Individual entry in the email unit credit ledger.
/// </summary>
public sealed class UnitLedgerEntry
{
    /// <summary>
    /// Gets or sets the ledger entry ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account ID.
    /// </summary>
    public long? AccountId { get; set; }

    /// <summary>
    /// Gets or sets the account email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the signed credit delta applied to the purchased balance.
    /// </summary>
    public long Quantity { get; set; }

    /// <summary>
    /// Gets or sets the resulting purchased credit balance after this entry.
    /// </summary>
    public long BalanceAfter { get; set; }

    /// <summary>
    /// Gets or sets the entry source (<c>purchase</c>, <c>charge</c>, <c>grant</c>, etc.).
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the external/internal reference identifier.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// Gets or sets the actor account ID if applicable.
    /// </summary>
    public long? ActorId { get; set; }

    /// <summary>
    /// Gets or sets the human-readable reason.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }
}

/// <summary>
/// Subscription plan catalog item.
/// </summary>
public sealed class Plan
{
    /// <summary>
    /// Gets or sets the plan ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plan display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the monthly USD price.
    /// </summary>
    public double UsdPrice { get; set; }

    /// <summary>
    /// Gets or sets the monthly NGN price.
    /// </summary>
    public double NairaPrice { get; set; }

    /// <summary>
    /// Gets or sets the yearly USD price.
    /// </summary>
    public double? YearlyUsdPrice { get; set; }

    /// <summary>
    /// Gets or sets the yearly NGN price.
    /// </summary>
    public double? YearlyNairaPrice { get; set; }

    /// <summary>
    /// Gets or sets optional multi-interval pricing metadata.
    /// </summary>
    public JsonElement? IntervalPrices { get; set; }

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
    /// Gets or sets the per-minute send rate limit.
    /// </summary>
    public int SendRateLimitPerMinute { get; set; }

    /// <summary>
    /// Gets or sets the log retention period in days.
    /// </summary>
    public int DataRetentionDays { get; set; }

    /// <summary>
    /// Gets or sets plan feature flags as JSON.
    /// </summary>
    public JsonElement? Features { get; set; }

    /// <summary>
    /// Gets or sets plan limit metadata as JSON.
    /// </summary>
    public JsonElement? Limits { get; set; }

    /// <summary>
    /// Gets or sets the overage rate per 1,000 emails in USD.
    /// </summary>
    public double OverageUsdPer1000 { get; set; }

    /// <summary>
    /// Gets or sets the dedicated IP addon price in USD.
    /// </summary>
    public double DedicatedIpAddonUsd { get; set; }

    /// <summary>
    /// Gets or sets the plan status.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Resolved effective limits for the authenticated account.
/// </summary>
public sealed class EffectiveLimits
{
    /// <summary>
    /// Gets or sets the account ID.
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// Gets or sets the active plan ID.
    /// </summary>
    public string PlanId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the per-minute send rate limit.
    /// </summary>
    public int SendRateLimitPerMinute { get; set; }

    /// <summary>
    /// Gets or sets the per-minute mailbox operations rate limit.
    /// </summary>
    public int MailboxOpsPerMinute { get; set; }

    /// <summary>
    /// Gets or sets the total mailbox limit (<c>-1</c> indicates unlimited).
    /// </summary>
    public int MailboxLimit { get; set; }

    /// <summary>
    /// Gets or sets the base mailbox limit included in the plan.
    /// </summary>
    public int BaseMailboxLimit { get; set; }

    /// <summary>
    /// Gets or sets purchased additional mailbox slots.
    /// </summary>
    public int AdditionalMailboxLimit { get; set; }

    /// <summary>
    /// Gets or sets whether custom domain sending is enabled.
    /// </summary>
    public bool CustomDomainSendingEnabled { get; set; }

    /// <summary>
    /// Gets or sets the sending domain limit.
    /// </summary>
    public int DomainLimit { get; set; }

    /// <summary>
    /// Gets or sets whether inbound receiving is enabled.
    /// </summary>
    public bool ReceivingEnabled { get; set; }

    /// <summary>
    /// Gets or sets the mailbox storage quota in gigabytes.
    /// </summary>
    public long MailboxStorageGb { get; set; }
}

/// <summary>
/// Wrapper envelope for effective account limits (<c>{"limits": {...}}</c>).
/// </summary>
public sealed class LimitsEnvelope
{
    /// <summary>
    /// Gets or sets the effective limits.
    /// </summary>
    public EffectiveLimits Limits { get; set; } = new();
}

/// <summary>
/// Plan block returned by <c>GET /status</c>.
/// </summary>
public sealed class AccountPlanStatus
{
    /// <summary>
    /// Gets or sets the plan ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plan name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscription status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the billing interval.
    /// </summary>
    public string BillingInterval { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    public string? EndDate { get; set; }

    /// <summary>
    /// Gets or sets whether auto-renewal is enabled.
    /// </summary>
    public bool AutoRenew { get; set; }
}

/// <summary>
/// Unit balance block returned by <c>GET /status</c>.
/// </summary>
public sealed class AccountUnitStatus
{
    /// <summary>
    /// Gets or sets the monthly allocation.
    /// </summary>
    public long MonthlyAllocation { get; set; }

    /// <summary>
    /// Gets or sets subscription units used.
    /// </summary>
    public long SubscriptionUsed { get; set; }

    /// <summary>
    /// Gets or sets subscription units remaining.
    /// </summary>
    public long SubscriptionRemaining { get; set; }

    /// <summary>
    /// Gets or sets purchased credit balance.
    /// </summary>
    public long PurchasedBalance { get; set; }

    /// <summary>
    /// Gets or sets total remaining sending capacity.
    /// </summary>
    public long TotalRemaining { get; set; }

    /// <summary>
    /// Gets or sets whether sending is unlimited.
    /// </summary>
    public bool Unlimited { get; set; }
}

/// <summary>
/// Account plan and credit status returned by <c>GET /status</c>.
/// </summary>
public sealed class AccountStatus
{
    /// <summary>
    /// Gets or sets the account plan status.
    /// </summary>
    public AccountPlanStatus Plan { get; set; } = new();

    /// <summary>
    /// Gets or sets the account unit balance status.
    /// </summary>
    public AccountUnitStatus Units { get; set; } = new();
}
