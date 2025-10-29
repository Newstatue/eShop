using System.Text.Json;

namespace Webhook.Models;

public sealed record KeycloakWebhookEvent(
    string EventType,
    string? UserId,
    string? RealmId,
    JsonElement Payload,
    string RawPayload,
    IReadOnlyDictionary<string, string> Headers)
{
    /// <summary>
    /// 结构化后的基础字段。Details 保留为原始 JsonElement，以便针对不同事件类型自定义解析。
    /// </summary>
    public KeycloakWebhookPayload? PayloadModel { get; init; }

    private static readonly JsonSerializerOptions PayloadSerializerOptions = new(JsonSerializerDefaults.Web);

    public static KeycloakWebhookEvent From(JsonElement root, string rawPayload, IReadOnlyDictionary<string, string> headers)
    {
        var eventType = root.TryGetProperty("type", out var typeElement)
            ? typeElement.GetString() ?? "unknown"
            : "unknown";

        string? userId = null;
        if (root.TryGetProperty("authDetails", out var authDetails) &&
            authDetails.TryGetProperty("userId", out var authUser))
        {
            userId = authUser.GetString();
        }
        else if (root.TryGetProperty("userId", out var userElement))
        {
            userId = userElement.GetString();
        }

        var realm = root.TryGetProperty("realmId", out var realmElement) ? realmElement.GetString() : null;
        var payloadClone = root.Clone();
        var headersSnapshot = headers is Dictionary<string, string> dictionary
            ? new Dictionary<string, string>(dictionary, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);

        KeycloakWebhookPayload? payloadModel = null;
        try
        {
            payloadModel = payloadClone.Deserialize<KeycloakWebhookPayload>(PayloadSerializerOptions);
        }
        catch (JsonException)
        {
            // 忽略解析错误，保留原始载荷以便诊断
        }

        return new KeycloakWebhookEvent(eventType, userId, realm, payloadClone, rawPayload, headersSnapshot)
        {
            PayloadModel = payloadModel
        };
    }
}
