namespace Basket.GrpcClients;

public class CatalogGrpcClient(CatalogService.CatalogServiceClient client)
{
    public async Task<Product> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new GetProductByIdRequest { Id = id };
        var response = await client.GetProductByIdAsync(request, cancellationToken: cancellationToken);

        return new Product
        {
            Id = response.Id,
            Name = response.Name,
            Description = response.Description,
            Price = (decimal)response.Price
        };
    }
}
