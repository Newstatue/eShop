namespace Catalog.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products").WithTags("Products");

        //GET查询所有商品
        group.MapGet("/", async (ProductService service, IMapper mapper) =>
            {
                var products = await service.GetProductsAsync();
                var response = mapper.Map<List<ProductResponse>>(products);
                return Results.Ok(response);
            })
            .WithName("GetAllProducts")
            .Produces<List<ProductResponse>>();

        //GET通过ID查询单个商品
        group.MapGet("/{id}", async (int id, ProductService service, IMapper mapper) =>
            {
                var product = await service.GetProductByIdAsync(id);
                if (product is null) return Results.NotFound();

                var response = mapper.Map<ProductResponse>(product);
                return Results.Ok(response);
            })
            .WithName("GetProductById")
            .Produces<ProductResponse>()
            .Produces(404);

        //POST创建商品
        group.MapPost("/", async (ProductUpsertRequest request, ProductService service, IMapper mapper) =>
            {
                var product = mapper.Map<Product>(request);

                try
                {
                    await service.CreateProductAsync(product);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }

                var response = mapper.Map<ProductResponse>(product);
                return Results.Created($"/products/{response.Id}", response);
            })
            .WithName("CreateProduct")
            .Produces<ProductResponse>(201);

        //PUT更新商品
        group.MapPut("/{id}", async (int id, ProductUpsertRequest request, ProductService service, IMapper mapper) =>
            {
                var updateProduct = await service.GetProductByIdAsync(id);
                if (updateProduct is null) return Results.NotFound();

                var inputProduct = mapper.Map<Product>(request);

                try
                {
                    await service.UpdateProductAsync(updateProduct, inputProduct);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }

                return Results.NoContent();
            })
            .WithName("UpdateProduct")
            .Produces(204)
            .Produces(404);

        //DELETE删除商品
        group.MapDelete("/{id}", async (int id, ProductService service) =>
            {
                var deleteProduct = await service.GetProductByIdAsync(id);
                if (deleteProduct is null) return Results.NotFound();

                await service.DeleteProductAsync(deleteProduct);
                return Results.NoContent();
            })
            .WithName("DeleteProduct")
            .Produces(204)
            .Produces(404);

        group.MapGet("/support/{query}", async (string query, ProductAIService service) =>
            {
                var response = await service.SupportAsync(query);
                return Results.Ok(response);
            })
            .WithName("Support")
            .Produces(200);

        //传统搜索
        group.MapGet("search/{query}", async (string query, ProductService service, IMapper mapper) =>
            {
                var products = await service.SearchProductsAsync(query);
                var response = mapper.Map<List<ProductResponse>>(products);
                return Results.Ok(response);
            })
            .WithName("SearchProducts")
            .Produces<List<ProductResponse>>();

        //AI搜索
        group.MapGet("aisearch/{query}", async (string query, ProductAIService service, IMapper mapper) =>
            {
                var products = await service.SearchProductsAsync(query);
                var response = mapper.Map<List<ProductResponse>>(products);
                return Results.Ok(response);
            })
            .WithName("AIProductSearch")
            .Produces<List<ProductResponse>>();
    }
}
