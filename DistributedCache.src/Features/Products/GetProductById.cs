using System.Text.Json;
using DistributedCache.Data;
using DistributedCache.Endpoints;
using DistributedCache.Models;
using DistributedCache.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace DistributedCache.Features.Products;

public class GetProductById : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:int}", async (int id, AppDbContext db, IDistributedCache cache, IOptions<CacheSettings> cacheSettings) =>
        {
            var settings = cacheSettings.Value;
            var cacheKey = $"{settings.ProductKeyPrefix}:{id}";

            var cachedJson = await cache.GetStringAsync(cacheKey);
            if (cachedJson is not null)
            {
                var cached = JsonSerializer.Deserialize<Product>(cachedJson);
                return Results.Ok(new { source = "cache", data = cached });
            }

            var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
                return Results.NotFound(new { message = "Product not found", id });

            var json = JsonSerializer.Serialize(product);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(settings.AbsoluteExpirationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(settings.SlidingExpirationMinutes)
            };
            await cache.SetStringAsync(cacheKey, json, options);

            return Results.Ok(new { source = "database", data = product });
        })
        .WithName("GetProduct");
    }
}
