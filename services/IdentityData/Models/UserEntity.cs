using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityData.Models;

[Table("Users")]
public class UserEntity
{
    [Key] [MaxLength(128)] public string KeycloakId { get; set; } = default!;

    [MaxLength(256)] public string? Username { get; set; }

    [MaxLength(100)] public string? LastName { get; set; }

    [MaxLength(100)] public string? FirstName { get; set; }

    [MaxLength(256)] public string? Email { get; set; }

    public bool EmailVerified { get; set; }

    [MaxLength(128)] public string? RealmId { get; set; }

    [MaxLength(100)] public string? CreatedClientId { get; set; }

    [MaxLength(45)] public string? CreatedFromIp { get; set; }

    [MaxLength(45)] public string? LastLoginIp { get; set; }
    [MaxLength(128)] public string? LastSessionId { get; set; }
    [MaxLength(50)] public string? RegisterMethod { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
