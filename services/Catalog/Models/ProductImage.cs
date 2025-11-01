using System.Text.Json.Serialization;

namespace Catalog.Models;

public class ProductImage
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int ProductId { get; set; }

    [JsonIgnore]
    public Product? Product { get; set; }
}
