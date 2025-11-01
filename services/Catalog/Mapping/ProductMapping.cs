namespace Catalog.Mapping;

public class ProductMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductResponse>()
            .Map(dest => dest.CategoryName, src => src.Category == null ? null : src.Category.Name)
            .Map(dest => dest.Tags, src => src.Tags.Select(tag => tag.Name).ToList())
            .Map(dest => dest.AiStatus, src => BuildAiStatus(src));

        config.NewConfig<ProductImage, ProductImageResponse>();
        config.NewConfig<ProductVariant, ProductVariantResponse>();

        config.NewConfig<ProductUpsertRequest, Product>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.Tags)
            .Map(dest => dest.UseAIGeneratedRichDescription, src => src.UseAIGeneratedRichDescription);
    }

    private static IReadOnlyList<string> BuildAiStatus(Product product)
    {
        var statuses = new List<string>();

        var hasTags = product.Tags.Any();
        var hasRichDescription = !string.IsNullOrWhiteSpace(product.RichDescription);

        if (!hasTags)
        {
            statuses.Add("GeneratingTags");
        }

        if (!hasRichDescription)
        {
            statuses.Add("GeneratingRichDescription");
        }

        if (hasTags && hasRichDescription)
        {
            statuses.Add("Completed");
        }

        return statuses;
    }
}
