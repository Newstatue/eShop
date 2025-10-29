namespace Basket.Services;

public interface IBasketService
{
    Task<ShoppingCart?> GetBasketAsync(string userName);
    Task UpdateBasketAsync(ShoppingCart basket);
    Task DeleteBasketAsync(string userName);
    Task UpdateBasketItemProductPrices(int productId, decimal newPrice);
}
