using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? RichDescription { get; set; }
    public bool IsRichDescriptionAIGenerated { get; set; } = false;

    public List<ProductImage> Images { get; set; } = new();

    public string? PrimaryImageUrl => Images.FirstOrDefault(i => i.IsPrimary)?.Url
                                      ?? Images.FirstOrDefault()?.Url;

    public int CategoryId { get; set; }
    public ProductCategory? Category { get; set; }
    public List<ProductTag> Tags { get; set; } = new();

    public decimal BasePrice { get; set; }

    [NotMapped]
    [Obsolete("Use BasePrice instead.")]
    public decimal Price
    {
        get => BasePrice;
        set => BasePrice = value;
    }

    [NotMapped]
    public bool UseAIGeneratedRichDescription { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ProductVariant> Variants { get; set; } = new();
}
