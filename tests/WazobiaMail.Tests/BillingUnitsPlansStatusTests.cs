using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace WazobiaMail.Tests;

public sealed class BillingUnitsPlansStatusTests
{
    [Fact]
    public async Task BillingUnitsPlansAndStatus_WorkAsExpected()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"inv","data":{"items":[{"id":"inv_1","accountId":1,"amount":10.0,"currency":"USD","status":"paid","purpose":"subscription"}],"pagination":{"page":1,"pageSize":20,"totalItems":1,"totalPages":1,"hasNext":false,"hasPrev":false}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"inv1","data":{"invoice":{"id":"inv_1","amount":10.0,"currency":"USD","status":"paid"}}}""");
        handler.EnqueueBytes(HttpStatusCode.OK, Encoding.UTF8.GetBytes("%PDF-invoice"), "application/pdf");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"sub","data":{"subscription":{"id":"sub_1","planId":"growth","status":"active","autoRenew":true,"billingInterval":"monthly"}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"cur","data":{"country":"NG","currency":"NGN"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"usum","data":{"includedAllowance":3000,"includedUsed":100,"includedRemaining":2900,"purchasedBalance":5000,"unlimited":false,"canBuyCredits":true}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"upacks","data":{"items":[{"id":"pack_1","name":"10k","units":10000,"priceNgnMinor":500000,"priceUsdMinor":400,"isActive":true,"sortOrder":1}]}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"uled","data":{"items":[{"id":"led_1","quantity":10000,"balanceAfter":15000,"source":"purchase","reference":"inv_2"}],"pagination":{"page":2,"pageSize":10,"totalItems":12,"totalPages":2,"hasNext":false,"hasPrev":true}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"upur","data":{"invoiceId":"inv_2","units":10000,"amountMinor":500000,"currency":"NGN","status":"pending_payment"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"plans","data":{"items":[{"id":"free","name":"Free","usdPrice":0,"nairaPrice":0,"monthlyQuota":3000,"status":"active"}],"pagination":{"page":1,"pageSize":20,"totalItems":1,"totalPages":1,"hasNext":false,"hasPrev":false}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"eff","data":{"limits":{"accountId":1,"planId":"free","sendRateLimitPerMinute":60,"mailboxLimit":2,"domainLimit":1,"receivingEnabled":true,"mailboxStorageGb":5}}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"success":true,"message":"status","data":{"plan":{"id":"free","name":"Free","status":"active"},"units":{"monthlyAllocation":3000,"totalRemaining":2900}}}""");

        using var http = new HttpClient(handler);
        using var client = new WazobiaMailClient(new WazobiaMailOptions { ApiKey = "wzbm_test_12345678" }, http);

        var invoices = await client.Billing.ListInvoicesAsync();
        Assert.Equal("inv_1", Assert.Single(invoices.Data.Items).Id);

        var invoice = await client.Billing.GetInvoiceAsync("inv_1");
        Assert.Equal("paid", invoice.Data.Invoice.Status);

        var pdf = await client.Billing.DownloadInvoiceAsync("inv_1");
        Assert.Equal("application/pdf", pdf.ContentType);
        Assert.Equal("%PDF-invoice", Encoding.UTF8.GetString(pdf.Content));

        var sub = await client.Billing.GetCurrentSubscriptionAsync("email");
        Assert.Equal("growth", sub.Data.Subscription?.PlanId);
        Assert.Equal("?product=email", handler.Requests[3].Uri.Query);

        var currency = await client.Billing.DetectCurrencyAsync();
        Assert.Equal("NGN", currency.Data.Currency);

        var summary = await client.Units.GetSummaryAsync();
        Assert.Equal(5000, summary.Data.PurchasedBalance);

        var packs = await client.Units.ListPacksAsync();
        Assert.Equal(10000, Assert.Single(packs.Data.Items).Units);

        var ledger = await client.Units.GetLedgerAsync(page: 2, perPage: 10);
        Assert.Equal(10000, Assert.Single(ledger.Data.Items).Quantity);
        Assert.Contains("page=2", handler.Requests[7].Uri.Query, StringComparison.Ordinal);
        Assert.Contains("per_page=10", handler.Requests[7].Uri.Query, StringComparison.Ordinal);

        var purchase = await client.Units.PurchaseAsync(new PurchaseUnitsRequest { PackId = "pack_1", Currency = UnitCurrency.Ngn });
        Assert.Equal("NGN", purchase.Data.Currency);
        using (var purchaseDoc = JsonDocument.Parse(handler.Requests[8].BodyBytes))
        {
            Assert.Equal("NGN", purchaseDoc.RootElement.GetProperty("currency").GetString());
        }

        var plans = await client.Plans.ListAsync();
        Assert.Equal("free", Assert.Single(plans.Data.Items).Id);

        var limits = await client.Plans.GetEffectiveLimitsAsync();
        Assert.Equal(60, limits.Data.Limits.SendRateLimitPerMinute);

        var status = await client.Status.GetAsync();
        Assert.Equal("free", status.Data.Plan.Id);
        Assert.Equal(2900, status.Data.Units.TotalRemaining);
    }
}
