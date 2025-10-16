namespace Basket.ApiClients;

public class CatalogApiClient(HttpClient httpClient)
{
    public async Task<Product> GetProductByIdAsync(int id)
    {
        var response = await httpClient.GetFromJsonAsync<Product>($"/products/{id}");
        return response!;
    }
}
