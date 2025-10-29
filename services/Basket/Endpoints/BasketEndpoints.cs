namespace Basket.Endpoints;

public static class BasketEndpoints
{
    public static void MapBasketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/basket").WithTags("Basket");

        //GET通过用户名查询购物车
        group.MapGet("/{userName}", async (string userName, IBasketService service) =>
            {
                var shoppingCart = await service.GetBasketAsync(userName);
                return shoppingCart is not null ? Results.Ok(shoppingCart) : Results.NotFound();
            })
            .WithName("GetBasket")
            .Produces<ShoppingCart>()
            .Produces(404)
            .RequireAuthorization();

        //POST更新购物车
        group.MapPost("/", async (ShoppingCart shoppingCart, IBasketService service) =>
            {
                await service.UpdateBasketAsync(shoppingCart);
                return Results.Created("GetBasket", shoppingCart);
            })
            .WithName("UpdateBasket")
            .Produces<ShoppingCart>(201)
            .RequireAuthorization();
        
        //DELETE通过用户名删除购物车
        group.MapDelete("/{userName}", async (string userName, IBasketService service) =>
            {
                await service.DeleteBasketAsync(userName);
                return Results.NoContent();
            })
            .WithName("DeleteBasket")
            .Produces(204)
            .RequireAuthorization();
    }
}
