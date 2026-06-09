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
        app.MapPut("/products/{id:int}", async (int id, ProductUpdateDto input, AppDbContext db, IDistributedCache cache, IOptions<CacheSettings> cacheSettings) =>
        {
            var product = await Execute(id, input, db, cacheSettings.Value, cache);
            if (product is null) return Results.NotFound(new { message = "Product not found", id });
            return Results.Ok(new { message = "Product updated successfully", product });
        })
        .WithName("UpdateProduct");
    }

    public static async Task<Product?> Execute(int id, ProductUpdateDto input, AppDbContext db, CacheSettings cacheSettings, IDistributedCache cache)
    {
        var product = await UpdateProductInDatabase(db, id, input);
        if (product is null) return null;
        await InvalidateProductCache(id, cacheSettings, cache);
        return product;
    }

    private static async Task<Product?> UpdateProductInDatabase(AppDbContext db, int id, ProductUpdateDto input)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return null;

        product.Price = input.Price;

        if (db.ChangeTracker.HasChanges())
        {
        await db.SaveChangesAsync();
        }
        return product;
    }

    private static async Task InvalidateProductCache(int id, CacheSettings settings, IDistributedCache cache)
    {
        var cacheKey = $"{settings.ProductKeyPrefix}:{id}";
        await cache.RemoveAsync(cacheKey);
    }
}

