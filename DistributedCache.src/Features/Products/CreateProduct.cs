using DistributedCache.Data;
using DistributedCache.Endpoints;
using DistributedCache.Models;

namespace DistributedCache.Features.Products;

public class CreateProduct : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (Product product, AppDbContext db) =>
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();

            return Results.Created($"/products/{product.Id}", product);
        })
        .WithName("CreateProduct");
    }
}
