namespace Catalog.EventHandler;

public class ProductCreatedEventHandler(
    ILogger<ProductCreatedEventHandler> logger,
    ProductDbContext dbContext,
    ProductAIService aiService
) : IConsumer<ProductCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        var evt = context.Message;

        try
        {
            logger.LogInformation("🧠 收到商品创建事件：{ProductId} - {ProductName}", evt.ProductId, evt.Name);

            var product = await dbContext.Products
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == evt.ProductId);

            if (product is null)
            {
                logger.LogWarning("⚠️ 找不到商品（ID={ProductId}），跳过处理。", evt.ProductId);
                return;
            }

            await aiService.ProcessNewProductAsync(
                product,
                persistChanges: true,
                regenerateRichDescription: evt.UseAIGeneratedRichDescription,
                cancellationToken: context.CancellationToken);

            logger.LogInformation("✅ 商品 {ProductId} 的标签与向量处理完毕。", evt.ProductId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ 处理商品创建事件失败（ID={ProductId}）。", evt.ProductId);
        }
    }
}
