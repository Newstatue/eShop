using System.Text.Json.Serialization;

namespace Catalog.Models;

public class ProductVariant
{
    public int Id { get; set; }
    public string Sku { get; set; } = null!; // 唯一 SKU 编号
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }

    public decimal Price { get; set; } // SKU 结算价，可能与商品显示价（BasePrice）不同
    public int StockQuantity { get; set; } // 当前库存
    public string? ImageUrl { get; set; } // SKU 专属图片

    // 关联 Product
    public int ProductId { get; set; }

    [JsonIgnore]
    public Product? Product { get; set; }
}
