namespace IdentityData.Models;

public class AuthDetails
{
    public string? RealmId { get; set; }
    public string? ClientId { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? Username { get; set; }
    public string? SessionId { get; set; }
}