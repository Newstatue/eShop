namespace Basket.Repositories;

public class RedisBasketRepository(IDistributedCache cache) : IBasketRepository
{
    public async Task<ShoppingCart?> GetBasketAsync(string userId)
    {
        var basket = await cache.GetStringAsync(userId);
        return string.IsNullOrEmpty(basket)
            ? null
            : JsonSerializer.Deserialize<ShoppingCart>(basket);
    }

    public async Task SaveBasketAsync(ShoppingCart basket) =>
        await cache.SetStringAsync(basket.UserId, JsonSerializer.Serialize(basket));

    public async Task DeleteBasketAsync(string userId)
    {
        await cache.RemoveAsync(userId);
    }
}
