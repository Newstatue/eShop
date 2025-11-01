namespace Basket.Exceptions;

public class CatalogProductNotFoundException(int productId)
    : Exception($"没有找到商品 {productId}")
{
    public int ProductId { get; } = productId;
}
