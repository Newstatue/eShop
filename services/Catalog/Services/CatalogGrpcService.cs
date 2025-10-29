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

        return new ProductReply
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = (double)product.Price
        };
    }
}
