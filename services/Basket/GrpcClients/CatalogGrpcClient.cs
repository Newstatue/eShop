using Grpc.Core;

namespace Basket.GrpcClients;

public class CatalogGrpcClient(CatalogService.CatalogServiceClient client)
{
    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new GetProductByIdRequest { Id = id };

        try
        {
            var response = await client.GetProductByIdAsync(request, cancellationToken: cancellationToken);

            var product = new Product
            {
                Id = response.Id,
                Name = response.Name,
                Description = response.Description,
                Brand = response.Brand,
                BasePrice = (decimal)response.DisplayPrice
            };

            product.Images.Clear();
            if (!string.IsNullOrWhiteSpace(response.PrimaryImageUrl))
            {
                product.Images.Add(new ProductImage
                {
                    Url = response.PrimaryImageUrl,
                    IsPrimary = true
                });
            }

            product.Variants.Clear();
            product.Variants.AddRange(response.Variants.Select(variant => new ProductVariant
            {
                Sku = variant.Sku,
                Price = (decimal)variant.Price,
                StockQuantity = variant.StockQuantity,
                ImageUrl = string.IsNullOrWhiteSpace(variant.ImageUrl) ? null : variant.ImageUrl,
                Color = string.IsNullOrWhiteSpace(variant.Color) ? null : variant.Color,
                Size = string.IsNullOrWhiteSpace(variant.Size) ? null : variant.Size,
                Material = string.IsNullOrWhiteSpace(variant.Material) ? null : variant.Material
            }));

            return product;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
