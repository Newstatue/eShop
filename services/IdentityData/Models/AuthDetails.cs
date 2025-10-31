using System.Text.Json.Serialization;

namespace IdentityData.Models;

public class AuthDetails
{
    [JsonPropertyName("realmId")]
    public string? RealmId { get; set; }

    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("ipAddress")]
    public string? IpAddress { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }
}
