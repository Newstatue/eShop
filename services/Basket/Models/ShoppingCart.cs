namespace Basket.Models;

public class ShoppingCart
{
    public string UserId { get; set; } = string.Empty;
    public List<ShoppingCartItem> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);
}
