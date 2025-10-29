using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;


using Microsoft.Extensions.Options;

using Webhook.Options;

namespace Webhook;

interface IKeycloakAdminTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

sealed class KeycloakAdminTokenProvider : IKeycloakAdminTokenProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<KeycloakOptions> _options;
    private readonly ILogger<KeycloakAdminTokenProvider> _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private TokenCache? _cache;

    public KeycloakAdminTokenProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<KeycloakOptions> options,
        ILogger<KeycloakAdminTokenProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var cache = Volatile.Read(ref _cache);
        if (cache is { } current && current.IsValid)
        {
            return current.AccessToken;
        }

        await _tokenLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cache = _cache;
            if (cache is { } refreshed && refreshed.IsValid)
            {
                return refreshed.AccessToken;
            }

            var tokenCache = await RequestTokenAsync(cancellationToken).ConfigureAwait(false);
            Volatile.Write(ref _cache, tokenCache);

            return tokenCache.AccessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<TokenCache> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var settings = _options.Value;

        if (string.IsNullOrWhiteSpace(settings.BaseAddress))
        {
            throw new InvalidOperationException("Keycloak.BaseAddress 不能为空。");
        }

        if (string.IsNullOrWhiteSpace(settings.Realm))
        {
            throw new InvalidOperationException("Keycloak.Realm 不能为空。");
        }

        if (string.IsNullOrWhiteSpace(settings.ClientId))
        {
            throw new InvalidOperationException("Keycloak.ClientId 不能为空。");
        }

        if (string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            throw new InvalidOperationException("Keycloak.ClientSecret 不能为空。");
        }

        using var client = CreateClient(settings.BaseAddress);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"realms/{settings.Realm}/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = settings.ClientId,
                ["client_secret"] = settings.ClientSecret
            })
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException($"获取 Keycloak 管理员访问令牌失败，状态码 {(int)response.StatusCode}，响应：{body}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("无法解析 Keycloak 的令牌响应。");
        }

        var expiresIn = tokenResponse.ExpiresIn;
        var refreshSkew = Math.Clamp(settings.RefreshSkewSeconds, 0, expiresIn);
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn - refreshSkew);

        _logger.LogInformation("已成功获取 Keycloak 管理员访问令牌，将在 {ExpiresAt:u} 前尝试刷新。", expiresAt);

        return new TokenCache(tokenResponse.AccessToken, expiresAt);
    }

    private HttpClient CreateClient(string baseAddress)
    {
        var client = _httpClientFactory.CreateClient(nameof(KeycloakAdminTokenProvider));
        if (client.BaseAddress is null || !string.Equals(client.BaseAddress.OriginalString, baseAddress, StringComparison.Ordinal))
        {
            client.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
        }

        return client;
    }

    private sealed record TokenCache(string AccessToken, DateTimeOffset ExpiresAt)
    {
        public bool IsValid => !string.IsNullOrEmpty(AccessToken) && DateTimeOffset.UtcNow < ExpiresAt;
    }

    private sealed record TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; init; } = string.Empty;

        [JsonPropertyName("scope")]
        public string Scope { get; init; } = string.Empty;
    }
}
