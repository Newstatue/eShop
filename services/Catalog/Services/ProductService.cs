namespace Catalog.Services;

public class ProductService(ProductDbContext dbContext, IBus bus)
{
    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Tags)
            .Include(p => p.Variants)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Tags)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task CreateProductAsync(Product product)
    {
        await EnsureCategoryExistsAsync(product.CategoryId);

        ValidateVariants(product.Variants);
        ValidateImages(product.Images);

        product.BasePrice = CalculateDisplayPrice(product.Variants);

        var wantsAiDescription = product.UseAIGeneratedRichDescription;

        if (wantsAiDescription)
        {
            product.IsRichDescriptionAIGenerated = false;
            product.RichDescription = null;
        }
        else
        {
            product.IsRichDescriptionAIGenerated = false;
            product.RichDescription = string.IsNullOrWhiteSpace(product.RichDescription)
                ? product.Description
                : product.RichDescription;
        }

        product.UseAIGeneratedRichDescription = false;

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        await dbContext.Entry(product).Reference(p => p.Category).LoadAsync();
        await dbContext.Entry(product).Collection(p => p.Images).LoadAsync();
        await dbContext.Entry(product).Collection(p => p.Tags).LoadAsync();
        await dbContext.Entry(product).Collection(p => p.Variants).LoadAsync();

        //发布商品成功创建事件
        var integrationEvent = new ProductCreatedIntegrationEvent()
        {
            ProductId = product.Id,
            Name = product.Name,
            Brand = product.Brand,
            Description = product.Description,
            BasePrice = product.BasePrice,
            UseAIGeneratedRichDescription = wantsAiDescription
        };
        await bus.Publish(integrationEvent);
    }

    public async Task UpdateProductAsync(Product existingProduct, Product inputProduct)
    {
        var originalBasePrice = existingProduct.BasePrice;

        ValidateVariants(inputProduct.Variants);
        ValidateImages(inputProduct.Images);

        var wantsAiDescription = inputProduct.UseAIGeneratedRichDescription;

        existingProduct.UseAIGeneratedRichDescription = false;

        existingProduct.Name = inputProduct.Name;
        existingProduct.Description = inputProduct.Description;
        existingProduct.Brand = inputProduct.Brand;

        if (existingProduct.CategoryId != inputProduct.CategoryId)
        {
            await EnsureCategoryExistsAsync(inputProduct.CategoryId);
            existingProduct.CategoryId = inputProduct.CategoryId;
        }

        existingProduct.IsActive = inputProduct.IsActive;

        if (wantsAiDescription)
        {
            existingProduct.IsRichDescriptionAIGenerated = false;
            existingProduct.RichDescription = null;
        }
        else if (!string.IsNullOrWhiteSpace(inputProduct.RichDescription))
        {
            existingProduct.IsRichDescriptionAIGenerated = false;
            existingProduct.RichDescription = inputProduct.RichDescription;
        }
        else if (string.IsNullOrWhiteSpace(existingProduct.RichDescription))
        {
            existingProduct.IsRichDescriptionAIGenerated = false;
            existingProduct.RichDescription = existingProduct.Description;
        }

        await dbContext.Entry(existingProduct).Collection(p => p.Images).LoadAsync();
        existingProduct.Images.Clear();
        foreach (var image in inputProduct.Images)
        {
            existingProduct.Images.Add(new ProductImage { Url = image.Url, IsPrimary = image.IsPrimary });
        }

        await dbContext.Entry(existingProduct).Collection(p => p.Variants).LoadAsync();
        existingProduct.Variants.Clear();
        foreach (var variant in inputProduct.Variants)
        {
            ValidateVariant(variant);

            existingProduct.Variants.Add(new ProductVariant
            {
                Sku = variant.Sku,
                Color = variant.Color,
                Size = variant.Size,
                Material = variant.Material,
                Price = variant.Price,
                StockQuantity = variant.StockQuantity,
                ImageUrl = variant.ImageUrl
            });
        }

        var recalculatedBasePrice = CalculateDisplayPrice(existingProduct.Variants);
        var priceChanged = originalBasePrice != recalculatedBasePrice;
        existingProduct.BasePrice = recalculatedBasePrice;

        await dbContext.SaveChangesAsync();


        //更新商品成功创建事件
        var updatedEvent = new ProductUpdatedIntegrationEvent()
        {
            ProductId = existingProduct.Id,
            Name = existingProduct.Name,
            Brand = existingProduct.Brand,
            Description = existingProduct.Description,
            BasePrice = existingProduct.BasePrice,
            UseAIGeneratedRichDescription = wantsAiDescription
        };
        await bus.Publish(updatedEvent);


        if (priceChanged)
        {
            var integrationEvent = new ProductPriceChangedIntegrationEvent
            {
                ProductId = existingProduct.Id,
                Name = existingProduct.Name,
                Description = existingProduct.Description,
                Price = existingProduct.BasePrice,
                ImageUrl = existingProduct.PrimaryImageUrl ?? string.Empty
            };

            await bus.Publish(integrationEvent);
        }
    }

    public async Task DeleteProductAsync(Product deletedProduct)
    {
        dbContext.Products.Remove(deletedProduct);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Name.Contains(query))
            .ToListAsync();
    }

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        if (!await dbContext.Categories.AnyAsync(c => c.Id == categoryId))
        {
            throw new InvalidOperationException($"categoryId '{categoryId}' does not exist.");
        }
    }

    private static decimal CalculateDisplayPrice(IEnumerable<ProductVariant> variants)
    {
        IEnumerable<ProductVariant> productVariants = variants as ProductVariant[] ?? [.. variants];
        return productVariants.Any()
            ? productVariants.Min(v => v.Price)
            : 0m;
    }

    private static void ValidateImages(IEnumerable<ProductImage> images)
    {
        var imageList = images as IList<ProductImage> ?? [.. images];
        var primaryCount = imageList.Count(img => img.IsPrimary);

        if (primaryCount > 1)
        {
            throw new InvalidOperationException("商品图片最多只能设置一个主图。");
        }
    }

    private static void ValidateVariants(IEnumerable<ProductVariant> variants)
    {
        var variantList = variants as IList<ProductVariant> ?? variants.ToList();
        if (variantList.Count == 0)
        {
            throw new InvalidOperationException("商品必须至少包含一个 SKU。");
        }

        foreach (var variant in variantList)
        {
            ValidateVariant(variant);
        }
    }

    private static void ValidateVariant(ProductVariant variant)
    {
        if (string.IsNullOrWhiteSpace(variant.Sku))
        {
            throw new InvalidOperationException("SKU 编号不能为空。");
        }

        if (variant.Price <= 0)
        {
            throw new InvalidOperationException($"SKU {variant.Sku} 的价格必须大于 0。");
        }
    }
}
