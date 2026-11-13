using System.Text.Json.Serialization;
using Xunit;

namespace WazobiaMail.Tests;

public sealed class WebhooksTests
{
    private sealed record SampleWebhookEvent(
        [property: JsonPropertyName("event")] string Event,
        [property: JsonPropertyName("trackingId")] string TrackingId);

    [Fact]
    public void SignAndVerifySignature_AcceptsPlainAndPrefixedHex_AndRejectsInvalid()
    {
        using var client = new WazobiaMailClient(new WazobiaMailOptions { AllowUnauthenticated = true });
        const string payload = """{"event":"message.delivered","trackingId":"trk_99"}""";
        const string secret = "whsec_test_secret_123";

        var sig = client.Webhooks.Sign(payload, secret);
        Assert.Equal(64, sig.Length);

        Assert.True(client.Webhooks.VerifySignature(payload, sig, secret));
        Assert.True(client.Webhooks.VerifySignature(payload, $"sha256={sig}", secret));
        Assert.True(client.Webhooks.VerifySignature(payload, $"SHA256={sig.ToUpperInvariant()}", secret));

        Assert.False(client.Webhooks.VerifySignature(payload + " ", sig, secret));
        Assert.False(client.Webhooks.VerifySignature(payload, sig, "wrong-secret"));
        Assert.False(client.Webhooks.VerifySignature(payload, "", secret));
        Assert.False(client.Webhooks.VerifySignature(payload, "not-hex", secret));
        Assert.False(client.Webhooks.VerifySignature(payload, sig, ""));
    }

    [Fact]
    public void ConstructEvent_DecodesTypedAndUntypedPayloads_AndThrowsOnErrors()
    {
        using var client = new WazobiaMailClient(new WazobiaMailOptions { AllowUnauthenticated = true });
        const string payload = """{"event":"message.delivered","trackingId":"trk_99"}""";
        const string secret = "whsec_test_secret_123";
        var sig = client.Webhooks.Sign(payload, secret);

        var typed = client.Webhooks.ConstructEvent<SampleWebhookEvent>(payload, sig, secret);
        Assert.Equal("message.delivered", typed.Event);
        Assert.Equal("trk_99", typed.TrackingId);

        var untyped = client.Webhooks.ConstructEvent(payload, $"sha256={sig}", secret);
        Assert.Equal("trk_99", untyped.GetProperty("trackingId").GetString());

        Assert.Throws<WazobiaWebhookException>(() => client.Webhooks.ConstructEvent(payload, "deadbeef", secret));

        var badJsonSig = client.Webhooks.Sign("not json", secret);
        Assert.Throws<WazobiaWebhookException>(() => client.Webhooks.ConstructEvent("not json", badJsonSig, secret));

        var arraySig = client.Webhooks.Sign("[1,2,3]", secret);
        Assert.Throws<WazobiaWebhookException>(() => client.Webhooks.ConstructEvent("[1,2,3]", arraySig, secret));
    }
}
