namespace Basket.Repositories;

public interface IBasketRepository
{
    Task<ShoppingCart?> GetBasketAsync(string userId);
    Task SaveBasketAsync(ShoppingCart basket);
    Task DeleteBasketAsync(string userId);
}
