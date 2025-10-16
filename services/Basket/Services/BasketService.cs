namespace Basket.Services;

public class BasketService(IDistributedCache cache, CatalogApiClient catalogApiClient)
{
    public async Task<ShoppingCart?> GetBasketAsync(string userName)
    {
        var basket = await cache.GetStringAsync(userName);
        return string.IsNullOrEmpty(basket) ? null 
            : JsonSerializer.Deserialize<ShoppingCart>(basket);
    }

    public async Task UpdateBasketAsync(ShoppingCart basket)
    {
        foreach (var item in basket.Items)
        {
            var product = await catalogApiClient.GetProductByIdAsync(item.ProductId);
            item.Price = product.Price;
            item.ProductName = product.Name;
        }
        await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket));
    }

    public async Task DeleteBasketAsync(string userName)
    {
        await cache.RemoveAsync(userName);
    }

    internal async Task UpdateBasketItemProductPrices(int productId, decimal newPrice)
    {
        //TODO: 这里仅作演示，实际项目中应该遍历所有用户的每个购物车项
        var basket = await GetBasketAsync("swn");
        var item = basket!.Items.FirstOrDefault(item => item.ProductId == productId);
        if (item is not null)
        {
            item.Price = newPrice;
            await  cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket));
        }
        
    }
}
