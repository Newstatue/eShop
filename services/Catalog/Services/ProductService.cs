namespace Catalog.Services;

public class ProductService(ProductDbContext dbContext, IBus bus)
{
    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        return await dbContext.Products.ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await dbContext.Products.FindAsync(id);
    }

    public async Task CreateProductAsync(Product product)
    {
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateProductAsync(Product updatedProduct, Product inputProduct)
    {
        // 如果价格变了，发布一个集成事件
        if (updatedProduct.Price != inputProduct.Price)
        {
            var integrationEvent = new ProductPriceChangedIntegrationEvent
            {
                ProductId = updatedProduct.Id,
                Name = inputProduct.Name,
                Description = inputProduct.Description,
                Price = inputProduct.Price,
                ImageUrl = inputProduct.ImageUrl
            };
            await bus.Publish(integrationEvent);
        }

        // 更新产品信息
        updatedProduct.Name = inputProduct.Name;
        updatedProduct.Description = inputProduct.Description;
        updatedProduct.ImageUrl = inputProduct.ImageUrl;
        updatedProduct.Price = inputProduct.Price;

        dbContext.Products.Update(updatedProduct);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(Product deletedProduct)
    {
        dbContext.Products.Remove(deletedProduct);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
    {
        return await dbContext.Products
            .Where(p => p.Name.Contains(query))
            .ToListAsync();
    }
}
