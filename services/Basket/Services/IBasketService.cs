namespace Basket.Services;

public interface IBasketService
{
    Task<ShoppingCart?> GetBasketAsync(string userId);
    Task UpdateBasketAsync(ShoppingCart basket);
    Task DeleteBasketAsync(string userId);
    Task UpdateBasketItemProductPrices(int productId, decimal newPrice);
}
