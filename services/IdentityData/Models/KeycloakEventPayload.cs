using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdentityData.Models;

public class KeycloakEventPayload
{
    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("realmId")]
    public string RealmId { get; set; } = string.Empty;

    [JsonPropertyName("uid")]
    public string Uid { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("authDetails")]
    public AuthDetails AuthDetails { get; set; } = new();

    [JsonPropertyName("details")]
    public JsonElement? Details { get; set; }
}
