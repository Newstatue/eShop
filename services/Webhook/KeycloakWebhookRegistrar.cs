using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

using Webhook.Options;

namespace Webhook;

sealed class KeycloakWebhookRegistrar(
    IKeycloakAdminTokenProvider tokenProvider,
    IOptions<KeycloakOptions> keycloakOptions,
    IOptions<WebhookOptions> webhookOptions,
    IHttpClientFactory httpClientFactory,
    ILogger<KeycloakWebhookRegistrar> logger) : IHostedService
{
    public const string HttpClientName = "KeycloakAdminApi";

    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var keycloak = keycloakOptions.Value;
        var webhook = webhookOptions.Value;

        if (string.IsNullOrWhiteSpace(keycloak.BaseAddress))
        {
            logger.LogWarning("未配置 Keycloak.BaseAddress，已跳过 Webhook 自动注册。");
            return;
        }

        if (string.IsNullOrWhiteSpace(keycloak.Realm))
        {
            throw new InvalidOperationException("Keycloak.Realm 不能为空。");
        }

        if (string.IsNullOrWhiteSpace(webhook.CallbackUrl))
        {
            logger.LogWarning("未配置 Webhook.CallbackUrl，已跳过自动注册。");
            return;
        }

        if (string.IsNullOrWhiteSpace(webhook.Secret))
        {
            throw new InvalidOperationException("Webhook.Secret 不能为空，请配置环境变量或 Aspire 参数。");
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        if (client.BaseAddress is null)
        {
            client.BaseAddress = new Uri(keycloak.BaseAddress, UriKind.Absolute);
        }

        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        var existing = await FindExistingWebhookAsync(client, keycloak.Realm, accessToken, webhook.CallbackUrl!, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            logger.LogInformation("检测到已存在的 Webhook (ID: {WebhookId})，不再重复创建。", existing.Id);
            return;
        }

        await CreateWebhookAsync(client, keycloak.Realm, accessToken, webhook, cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task<WebhookRepresentation?> FindExistingWebhookAsync(
        HttpClient client,
        string realm,
        string accessToken,
        string callbackUrl,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"realms/{realm}/webhooks");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            logger.LogWarning("查询 Webhook 列表失败，状态码 {StatusCode}，响应：{Body}", (int)response.StatusCode, body);
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var webhooks = await JsonSerializer.DeserializeAsync<List<WebhookRepresentation>>(stream, _serializerOptions, cancellationToken)
            .ConfigureAwait(false);

        return webhooks?
            .FirstOrDefault(h => string.Equals(h.Url, callbackUrl, StringComparison.OrdinalIgnoreCase));
    }

    private async Task CreateWebhookAsync(
        HttpClient client,
        string realm,
        string accessToken,
        WebhookOptions webhook,
        CancellationToken cancellationToken)
    {
        var requestBody = new WebhookCreateRequest(
            Enabled: true,
            Url: webhook.CallbackUrl!,
            Secret: webhook.Secret!,
            EventTypes: webhook.EventTypes is { Length: > 0 } ? webhook.EventTypes : new[] { "*" });

        var payload = JsonSerializer.Serialize(requestBody, _serializerOptions);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"realms/{realm}/webhooks")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("已在 Keycloak 注册 Webhook：{Url}", webhook.CallbackUrl);
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new InvalidOperationException($"注册 Keycloak Webhook 失败，状态码 {(int)response.StatusCode}，响应：{body}");
    }

    private sealed record WebhookRepresentation
    {
        [JsonPropertyName("id")] public string? Id { get; init; }

        [JsonPropertyName("url")] public string? Url { get; init; }

        [JsonPropertyName("enabled")] public bool Enabled { get; init; }
    }

    private sealed record WebhookCreateRequest(
        [property: JsonPropertyName("enabled")]
        bool Enabled,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("secret")] string Secret,
        [property: JsonPropertyName("eventTypes")]
        string[] EventTypes);
}
