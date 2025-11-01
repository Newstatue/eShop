namespace Catalog.EventHandler;

public class ProductUpdatedEventHandler(
    ILogger<ProductUpdatedEventHandler> logger,
    ProductDbContext dbContext,
    ProductAIService aiService) : IConsumer<ProductUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedIntegrationEvent> context)
    {
        var evt = context.Message;

        if (!evt.UseAIGeneratedRichDescription)
        {
            logger.LogDebug("跳过商品 {ProductId} 的 AI 描述更新（未请求）。", evt.ProductId);
            return;
        }

        try
        {
            logger.LogInformation("🧠 收到商品更新事件，准备重新生成描述：{ProductId}", evt.ProductId);

            var product = await dbContext.Products
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == evt.ProductId, context.CancellationToken);

            if (product is null)
            {
                logger.LogWarning("⚠️ 找不到商品（ID={ProductId}），跳过 AI 描述更新。", evt.ProductId);
                return;
            }

            product.IsRichDescriptionAIGenerated = false;
            await aiService.GenerateRichDescriptionAsync(product, context.CancellationToken);
            await dbContext.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation("📝 商品 {ProductId} 的 AI 描述更新完成。", evt.ProductId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ 处理商品更新事件失败（ID={ProductId}）。", evt.ProductId);
        }
    }
}
