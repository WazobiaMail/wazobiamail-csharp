using System.Text.Json;
using WazobiaMail.Http;

namespace WazobiaMail;

/// <summary>
/// Account authentication, token refresh, registration, and profile retrieval.
/// </summary>
public sealed class AuthResource
{
    private readonly WazobiaHttpTransport _transport;

    internal AuthResource(WazobiaHttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Authenticates an account with email and password (and optional TOTP code) to obtain JWT tokens.
    /// </summary>
    public Task<ApiResponse<AuthPayload>> LoginAsync(
        LoginRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<AuthPayload>(
            HttpMethod.Post,
            "/auth/login",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Registers a new WazobiaMail account.
    /// </summary>
    public Task<ApiResponse<JsonElement>> RegisterAsync(
        RegisterRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _transport.SendJsonAsync<JsonElement>(
            HttpMethod.Post,
            "/auth/register",
            request,
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Exchanges a refresh token for a new access token pair.
    /// </summary>
    public Task<ApiResponse<AuthPayload>> RefreshAsync(
        string refreshToken,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        return _transport.SendJsonAsync<AuthPayload>(
            HttpMethod.Post,
            "/auth/refresh",
            new RefreshTokenRequest { RefreshToken = refreshToken },
            options: options,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves the currently authenticated account profile and quota summary.
    /// </summary>
    public Task<ApiResponse<CurrentAccount>> MeAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.SendJsonAsync<CurrentAccount>(
            HttpMethod.Get,
            "/auth/me",
            options: options,
            cancellationToken: cancellationToken);
    }
}
