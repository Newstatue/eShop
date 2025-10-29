namespace Basket.EventHandlers;

public class ProductPriceChangedIntegrationEventHandler(IBasketService service)
    : IConsumer<ProductPriceChangedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductPriceChangedIntegrationEvent> context)
    {
        // 更新购物车中对应商品的价格
        await service.UpdateBasketItemProductPrices(context.Message.ProductId, context.Message.Price);
    }
}
