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
           var response = await Execute(id,db,cache,cacheSettings.Value);
            if (response.isFound)
            {
                return Results.Ok(new { response.source ,response.data });
            }
            return Results.NotFound(new {message="Product Not Found"});
        })
        .WithName("GetProduct");
    }
    public async static Task<Product?> SearchCache(IDistributedCache cache, string id)
    {
        var cacheString = await cache.GetStringAsync(id);
        if (cacheString is not null)
        {
            return JsonSerializer.Deserialize<Product>(cacheString);
        }
        return null;
    }
    public static Task SetCache(IDistributedCache cache, Product product, CacheSettings settings, string cacheKey)
    {
        var json = JsonSerializer.Serialize(product);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(settings.AbsoluteExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(settings.SlidingExpirationMinutes)
        };
        return cache.SetStringAsync(cacheKey, json, options);
    }
    public async static Task<Product?> SearchDb(AppDbContext db, int id)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return product;
    }
    public async static Task<ResponseSource> Execute(int id, AppDbContext db, IDistributedCache cache,CacheSettings cacheSettings)
    {
        var settings = cacheSettings;
        var cacheKey = $"{settings.ProductKeyPrefix}:{id}";
        var productCached = await SearchCache(cache, cacheKey);
        if (productCached is not null) return new ResponseSource("cache", productCached, true);
        var productDb = await SearchDb(db, id);
        if (productDb is null) return new ResponseSource("_blank", default, false);
        await SetCache(cache, productDb, cacheSettings, cacheKey);
        return new ResponseSource("database",productDb,true);
    }
    public record ResponseSource(string source, Product? data, bool isFound);
}
