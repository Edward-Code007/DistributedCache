using System.Text.Json;
using DistributedCache.Data;
using DistributedCache.Endpoints;
using DistributedCache.Models;
using DistributedCache.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace DistributedCache.Features.Products;

public class DeleteProduct : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id:int}", async (int id, AppDbContext db, IDistributedCache cache, IOptions<CacheSettings> cacheSettings) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null)
                return Results.NotFound(new { message = "Product not found", id });

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            var settings = cacheSettings.Value;
            var cacheKey = $"{settings.ProductKeyPrefix}:{id}";
            var json = JsonSerializer.Serialize(product);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(settings.AbsoluteExpirationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(settings.SlidingExpirationMinutes)
            };
            await cache.SetStringAsync(cacheKey, json, options);

            return Results.NoContent();
        })
        .WithName("DeleteProduct");
    }
}
