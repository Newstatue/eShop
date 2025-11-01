using Basket.Exceptions;

namespace Basket.Services;

public class BasketService(IBasketRepository repository, CatalogGrpcClient catalogClient)
    : IBasketService
{
    public Task<ShoppingCart?> GetBasketAsync(string userId) =>
        repository.GetBasketAsync(userId);

    public async Task UpdateBasketAsync(ShoppingCart basket)
    {
        foreach (var item in basket.Items)
        {
            var product = await catalogClient.GetProductByIdAsync(item.ProductId);
            if (product is null)
            {
                throw new CatalogProductNotFoundException(item.ProductId);
            }

            item.Price = product.Price;
            item.ProductName = product.Name;
        }

        await repository.SaveBasketAsync(basket);
    }

    public Task DeleteBasketAsync(string userId) =>
        repository.DeleteBasketAsync(userId);

    public async Task UpdateBasketItemProductPrices(int productId, decimal newPrice)
    {
        // TODO: 示例实现：目前仅更新用户 ID 为 "swn" 的购物车
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
