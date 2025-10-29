using System.Text.Json;

namespace IdentityData.Models;

public class KeycloakEventPayload
{
    public long Time { get; set; }
    public string RealmId { get; set; } = string.Empty;
    public string Uid { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public AuthDetails AuthDetails { get; set; } = new();
    public JsonElement? Details { get; set; }
}
