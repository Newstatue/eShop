using System.Security.Claims;
using Basket.Exceptions;

namespace Basket.Endpoints;

public static class BasketEndpoints
{
    public static void MapBasketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/basket").WithTags("Basket").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IBasketService service) =>
            {
                var userId = ResolveUserId(user);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var shoppingCart = await service.GetBasketAsync(userId);
                return shoppingCart is not null ? Results.Ok(shoppingCart) : Results.NotFound();
            })
            .WithName("GetBasket")
            .Produces<ShoppingCart>()
            .Produces(401)
            .Produces(404);

        group.MapPost("/", async (ClaimsPrincipal user, ShoppingCart shoppingCart, IBasketService service) =>
            {
                var userId = ResolveUserId(user);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                shoppingCart.UserId = userId;

                try
                {
                    await service.UpdateBasketAsync(shoppingCart);
                    return Results.Created("GetBasket", shoppingCart);
                }
                catch (CatalogProductNotFoundException ex)
                {
                    return Results.NotFound(new { ex.Message });
                }
            })
            .WithName("UpdateBasket")
            .Produces<ShoppingCart>(201)
            .Produces(401)
            .Produces(404);

        group.MapDelete("/", async (ClaimsPrincipal user, IBasketService service) =>
            {
                var userId = ResolveUserId(user);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                await service.DeleteBasketAsync(userId);
                return Results.NoContent();
            })
            .WithName("DeleteBasket")
            .Produces(204)
            .Produces(401);
    }

    private static string? ResolveUserId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier);
}
