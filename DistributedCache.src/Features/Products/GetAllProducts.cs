using DistributedCache.Data;
using DistributedCache.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace DistributedCache.Features.Products;

public class GetAllProducts : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (AppDbContext db) =>
        {
            var products = await db.Products.AsNoTracking().ToListAsync();
            return Results.Ok(products);
        })
        .WithName("GetAllProducts");
    }
}
