using DistributedCache.Data;
using DistributedCache.Endpoints;
using DistributedCache.Models;
using DistributedCache.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace DistributedCache.Features.Products;

public class UpdateProduct : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{id:int}", async (int id, Product input, AppDbContext db, IDistributedCache cache, IOptions<CacheSettings> cacheSettings) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null)
                return Results.NotFound(new { message = "Product not found", id });

            product.Name = input.Name;
            product.Description = input.Description;
            product.Price = input.Price;
            product.Stock = input.Stock;

            await db.SaveChangesAsync();

            await cache.RemoveAsync($"{cacheSettings.Value.ProductKeyPrefix}:{id}");

            return Results.Ok(product);
        })
        .WithName("UpdateProduct");
    }
}
