using DistributedCache.Features.Products;
using DistributedCache.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace DistributedCache.test;

public class IntegrationTest(RedisFixture redisFixture, PostgresFixture postgresFixture)
: IClassFixture<PostgresFixture>, IClassFixture<RedisFixture>
{
    public readonly PostgresFixture _postgresFixure = postgresFixture;
    public readonly RedisFixture _redisFixure = redisFixture;

    private IDistributedCache BuildCache() =>
        new RedisCache(new RedisCacheOptions { Configuration = _redisFixure.ConnectionString });

    [Fact]
    public async Task CreateProduct_ShouldCreateProductNInsertIntoDB()
    {
        var product = new Product()
        {
            Id = 200,
            Name = "PcMsi",
            Price = 1300,
            Description = "NasaPc",
            Stock = 5
        };
        await CreateProduct.AddProduct(product, _postgresFixure._dbContext);
        var productInDB = await _postgresFixure._dbContext.Products.FirstOrDefaultAsync(x => x.Id == 200);
        Assert.IsType<Product>(productInDB);
        Assert.Equal("NasaPc", productInDB.Description);
    }

    [Fact]
    public async Task RetrieveProductById_ShouldReturnFromDatabase_ThenFromCache()
    {
        IDistributedCache distCache = BuildCache();

        var firstResult = await GetProductById.Execute(2, _redisFixure._cacheSettings, distCache, _postgresFixure._dbContext);
        Assert.NotNull(firstResult);
        Assert.True(firstResult.isFound);
        Assert.Equal("database", firstResult.source);

        var secondResult = await GetProductById.Execute(2, _redisFixure._cacheSettings, distCache, _postgresFixure._dbContext);
        Assert.NotNull(secondResult);
        Assert.True(secondResult.isFound);
        Assert.Equal("cache", secondResult.source);
    }

    [Fact]
    public async Task RetrieveProductById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        IDistributedCache distCache = BuildCache();

        var result = await GetProductById.Execute(900, _redisFixure._cacheSettings, distCache, _postgresFixure._dbContext);
        Assert.NotNull(result);
        Assert.False(result.isFound);
        Assert.Equal("_blank", result.source);
    }

    [Fact]
    public async Task UpdateProduct_ShouldUpdatePriceAndInvalidateCache()
    {
        IDistributedCache distCache = BuildCache();

        // Warm up cache for product 4
        await GetProductById.Execute(4, _redisFixure._cacheSettings, distCache, _postgresFixure._dbContext);

        var input = new ProductUpdateDto { Price = 399.99m };
        var updated = await UpdateProduct.Execute(4, input, _postgresFixure._dbContext, _redisFixure._cacheSettings, distCache);

        Assert.NotNull(updated);
        Assert.Equal(399.99m, updated.Price);

        // Cache invalidated — next call must come from database
        var afterUpdate = await GetProductById.Execute(4, _redisFixure._cacheSettings, distCache, _postgresFixure._dbContext);
        Assert.Equal("database", afterUpdate.source);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnNull_WhenProductDoesNotExist()
    {
        IDistributedCache distCache = BuildCache();

        var result = await UpdateProduct.Execute(9999, new ProductUpdateDto { Price = 1m }, _postgresFixure._dbContext, _redisFixure._cacheSettings, distCache);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteProduct_ShouldSoftDelete_AndNotAppearInGetAll()
    {
        var deleted = await DeleteProduct.FindUserNDeleteDatabase(_postgresFixure._dbContext, 5);

        Assert.NotNull(deleted);
        Assert.True(deleted.IsDeleted);
        Assert.NotNull(deleted.DeletedAt);

        var allProducts = await GetAllProducts.RetrieveAllProducts(_postgresFixure._dbContext);
        Assert.DoesNotContain(allProducts, p => p.Id == 5);
    }
}
