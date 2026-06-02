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
            var product = await FindUserNDeleteDatabase(db, id);
            if (product is null) return Results.NotFound(new { message = "Product not found", id });
            await FindUserNSoftDeleteCache(id, cacheSettings.Value, product, cache);
            return Results.Ok("Log Deleted");
        })
        .WithName("DeleteProduct");
    }
    public async static Task<Product?> FindUserNDeleteDatabase(AppDbContext db, int id)
    {
        var product = await db.Products.FindAsync(id);
        if(product is null) return null;
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return product;
    }
    public async static Task FindUserNSoftDeleteCache(int id, CacheSettings cacheSettings, Product product, IDistributedCache cache)
    {
        var settings = cacheSettings;
        var cacheKey = $"{settings.ProductKeyPrefix}:{id}";
        var json = JsonSerializer.Serialize(product);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(settings.AbsoluteExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(settings.SlidingExpirationMinutes)
        };
        await cache.SetStringAsync(cacheKey, json, options);
    }
}
