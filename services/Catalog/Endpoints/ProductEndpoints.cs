namespace Catalog.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products").WithTags("Products");

        //GET查询所有商品
        group.MapGet("/", async (ProductService service) =>
            {
                var products = await service.GetProductsAsync();
                return Results.Ok(products);
            })
            .WithName("GetAllProducts")
            .Produces<List<Product>>();

        //GET通过ID查询单个商品
        group.MapGet("/{id}", async (int id, ProductService service) =>
            {
                var product = await service.GetProductByIdAsync(id);
                return product is not null ? Results.Ok(product) : Results.NotFound();
            })
            .WithName("GetProductById")
            .Produces<Product>()
            .Produces(404);

        //POST创建商品
        group.MapPost("/", async (Product inputProduct, ProductService service) =>
            {
                await service.CreateProductAsync(inputProduct);
                return Results.Created($"/products/{inputProduct.Id}", inputProduct);
            })
            .WithName("CreateProduct")
            .Produces<Product>(201);

        //PUT更新商品
        group.MapPut("/{id}", async (int id, Product inputProduct, ProductService service) =>
        {
            var updateProduct = await service.GetProductByIdAsync(id);
            if (updateProduct is null) return Results.NotFound();

            await service.UpdateProductAsync(updateProduct, inputProduct);
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
    }
}
