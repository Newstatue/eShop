namespace Basket.Models;

public class ShoppingCartItem
{
    public int Quantity { get; set; }
    public string Color { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public string ProductName { get; set; } = string.Empty;
}
