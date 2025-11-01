namespace Catalog.Contracts;

public sealed class ProductResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public string? RichDescription { get; init; }
    public bool IsRichDescriptionAIGenerated { get; init; } = false;
    public decimal BasePrice { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? PrimaryImageUrl { get; init; }
    public int CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public List<ProductImageResponse> Images { get; init; } = [];
    public List<ProductVariantResponse> Variants { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public IReadOnlyList<string> AiStatus { get; init; } = Array.Empty<string>();
}

public sealed class ProductImageResponse
{
    public string Url { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}

public sealed class ProductVariantResponse
{
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public string? ImageUrl { get; init; }
    public string? Color { get; init; }
    public string? Size { get; init; }
    public string? Material { get; init; }
}

public sealed class ProductUpsertRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public string? RichDescription { get; init; }
    public bool UseAIGeneratedRichDescription { get; init; } = false;
    public bool IsActive { get; init; } = true;
    public int CategoryId { get; init; }
    public List<ProductImageRequest> Images { get; init; } = [];
    public List<ProductVariantRequest> Variants { get; init; } = [];
}

public sealed class ProductImageRequest
{
    public string Url { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}

public sealed class ProductVariantRequest
{
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public string? ImageUrl { get; init; }
    public string? Color { get; init; }
    public string? Size { get; init; }
    public string? Material { get; init; }
}


