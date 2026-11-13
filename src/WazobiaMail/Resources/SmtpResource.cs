using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Customer SMTP relay configuration and account SMTP sending access management.
/// </summary>
public sealed class SmtpResource
{
    private readonly WazobiaHttpTransport _transport;

    internal SmtpResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Lists configured external SMTP relays.
    /// </summary>
    public Task<ApiResponse<SmtpRelaysEnvelope>> ListRelaysAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<SmtpRelaysEnvelope>(
            HttpMethod.Get,
            "/smtp-relays",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates a new external SMTP relay configuration.
    /// </summary>
    public Task<ApiResponse<SmtpRelayEnvelope>> CreateRelayAsync(
        CreateSmtpRelayRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<SmtpRelayEnvelope>(
            HttpMethod.Post,
            "/smtp-relays",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves a single SMTP relay by ID.
    /// </summary>
    public Task<ApiResponse<SmtpRelayEnvelope>> GetRelayAsync(
        string relayId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relayId);
        return _transport.SendJsonAsync<SmtpRelayEnvelope>(
            HttpMethod.Get,
            $"/smtp-relays/{Uri.EscapeDataString(relayId)}",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Partially updates an external SMTP relay configuration.
    /// </summary>
    public Task<ApiResponse<SmtpRelayEnvelope>> UpdateRelayAsync(
        string relayId,
        UpdateSmtpRelayRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relayId);
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<SmtpRelayEnvelope>(
            HttpMethod.Patch,
            $"/smtp-relays/{Uri.EscapeDataString(relayId)}",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes an SMTP relay configuration.
    /// </summary>
    public Task<ApiResponse<Dictionary<string, string>>> DeleteRelayAsync(
        string relayId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relayId);
        return _transport.SendJsonAsync<Dictionary<string, string>>(
            HttpMethod.Delete,
            $"/smtp-relays/{Uri.EscapeDataString(relayId)}",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Activates an SMTP relay.
    /// </summary>
    public Task<ApiResponse<SmtpRelayEnvelope>> ActivateRelayAsync(
        string relayId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relayId);
        return _transport.SendJsonAsync<SmtpRelayEnvelope>(
            HttpMethod.Post,
            $"/smtp-relays/{Uri.EscapeDataString(relayId)}/activate",
            new { },
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deactivates an SMTP relay.
    /// </summary>
    public Task<ApiResponse<SmtpRelayEnvelope>> DeactivateRelayAsync(
        string relayId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relayId);
        return _transport.SendJsonAsync<SmtpRelayEnvelope>(
            HttpMethod.Post,
            $"/smtp-relays/{Uri.EscapeDataString(relayId)}/deactivate",
            new { },
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Inspects account-wide SMTP sending eligibility and connection settings.
    /// </summary>
    public Task<ApiResponse<SmtpAccess>> GetAccessAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<SmtpAccess>(
            HttpMethod.Get,
            "/sending/smtp-access",
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Enables or disables account-wide SMTP sending access.
    /// </summary>
    public Task<ApiResponse<Dictionary<string, bool>>> UpdateAccessAsync(
        bool enabled,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<Dictionary<string, bool>>(
            HttpMethod.Put,
            "/sending/smtp-access",
            new { enabled },
            options: options,
            cancellationToken: cancellationToken);
    }
}
