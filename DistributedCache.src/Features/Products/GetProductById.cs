using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using DistributedCache.Data;
using DistributedCache.Endpoints;
using DistributedCache.Models;
using DistributedCache.Settings;
using Microsoft.AspNetCore.Http.HttpResults;
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
          var result = await Execute(id,cacheSettings.Value,cache,db);
            if (result.isFound)
            {
                return Results.Ok(new
                {
                   result.source,
                   result.product 
                });
            }
            return Results.NotFound("Not Found");
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
    public async static Task<ProductResponse> Execute(int id,CacheSettings cacheSettings,IDistributedCache cache,AppDbContext db)
    {
           var settings = cacheSettings;
            var cacheKey = $"{settings.ProductKeyPrefix}:{id}";

            var productCached = await SearchCache(cache, cacheKey);
            if (productCached is not null) return new ProductResponse("cache",productCached,true);

            var productDb = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (productDb is null) return new ProductResponse("_blank",null,false);

            await SetCache(cache,productDb,cacheSettings,cacheKey);
            return new ProductResponse("database",productDb,true);
    }
    public record ProductResponse(string source,Product? product, bool isFound );
}
