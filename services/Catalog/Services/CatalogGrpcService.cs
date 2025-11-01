using Catalog.Grpc;
using Grpc.Core;

namespace Catalog.Services;

public class CatalogGrpcService(ProductService productService) : CatalogService.CatalogServiceBase
{
    public override async Task<ProductReply> GetProductById(GetProductByIdRequest request, ServerCallContext context)
    {
        var product = await productService.GetProductByIdAsync(request.Id);
        if (product is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product {request.Id} not found."));
        }

        var reply = new ProductReply
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Brand = product.Brand,
            DisplayPrice = (double)product.BasePrice,
            PrimaryImageUrl = product.PrimaryImageUrl ?? string.Empty
        };

        reply.Variants.AddRange(product.Variants.Select(variant => new ProductVariantReply
        {
            Sku = variant.Sku,
            Price = (double)variant.Price,
            StockQuantity = variant.StockQuantity,
            ImageUrl = variant.ImageUrl ?? string.Empty,
            Color = variant.Color ?? string.Empty,
            Size = variant.Size ?? string.Empty,
            Material = variant.Material ?? string.Empty
        }));

        return reply;
    }
}
