using System.Reflection.Metadata.Ecma335;
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

            var productCached = await SearchCache(cache, cacheKey);
            if (productCached is not null) return Results.Ok(new { source = "cache", data = productCached });

            var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return Results.NotFound(new { message = "Product not found", id });
            
            await SetCache(cache,product,cacheSettings.Value,cacheKey);
            return Results.Ok(new { source = "database", data = product });
        })
        .WithName("GetProduct");
    }
    public async static Task<Product?> SearchCache(IDistributedCache cache, string id){
        var cacheString = await cache.GetStringAsync(id);
        if (cacheString is not null)
        {
            return JsonSerializer.Deserialize<Product>(cacheString);
        }
        return null;
    }
    public static Task SetCache(IDistributedCache cache, Product product, CacheSettings settings, string cacheKey){
        var json = JsonSerializer.Serialize(product);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(settings.AbsoluteExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(settings.SlidingExpirationMinutes)
        };
        return cache.SetStringAsync(cacheKey, json, options);
    }

}
