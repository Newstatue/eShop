namespace Basket.Services;

public class BasketService(IBasketRepository repository, CatalogGrpcClient catalogClient)
    : IBasketService
{
    public Task<ShoppingCart?> GetBasketAsync(string userName) =>
        repository.GetBasketAsync(userName);

    public async Task UpdateBasketAsync(ShoppingCart basket)
    {
        foreach (var item in basket.Items)
        {
            var product = await catalogClient.GetProductByIdAsync(item.ProductId);
            item.Price = product.Price;
            item.ProductName = product.Name;
        }

        await repository.SaveBasketAsync(basket);
    }

    public Task DeleteBasketAsync(string userName) =>
        repository.DeleteBasketAsync(userName);

    public async Task UpdateBasketItemProductPrices(int productId, decimal newPrice)
    {
        // TODO: 示例实现：目前仅更新用户名为 "swn" 的购物车
        var basket = await repository.GetBasketAsync("swn");
        if (basket is null)
        {
            return;
        }

        var item = basket.Items.FirstOrDefault(item => item.ProductId == productId);
        if (item is null)
        {
            return;
        }

        item.Price = newPrice;
        await repository.SaveBasketAsync(basket);
    }
}
