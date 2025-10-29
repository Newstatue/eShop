using System.ComponentModel.DataAnnotations;

namespace IdentityData.Models;

public class ProcessedEventEntity
{
    [Key] [MaxLength(64)] public string Uid { get; set; } = default!;
    public required string EventType { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
