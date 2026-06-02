using DistributedCache.Data;
using DistributedCache.Endpoints;
using DistributedCache.Models;
using Microsoft.EntityFrameworkCore;

namespace DistributedCache.Features.Products;

public class GetAllProducts : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (AppDbContext db) =>
        {
            var products = await RetrieveAllProducts(db);
            return Results.Ok(products);
        })
        .WithName("GetAllProducts");
    }
    public async static Task<List<Product>> RetrieveAllProducts(AppDbContext db)
    {
        return await db.Products.AsNoTracking().ToListAsync();
    }
}
