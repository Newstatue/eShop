namespace Basket.Repositories;

public interface IBasketRepository
{
    Task<ShoppingCart?> GetBasketAsync(string userName);
    Task SaveBasketAsync(ShoppingCart basket);
    Task DeleteBasketAsync(string userName);
}
