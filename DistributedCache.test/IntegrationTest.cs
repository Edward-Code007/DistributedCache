using DistributedCache.Features.Products;
using DistributedCache.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Testcontainers.Redis;

namespace DistributedCache.test;

public class IntegrationTest(RedisFixture redisFixture, PostgresFixture postgresFixture)
: IClassFixture<PostgresFixture>, IClassFixture<RedisFixture>
{
  public readonly PostgresFixture _postgresFixure = postgresFixture;
  public readonly RedisFixture _redisFixure = redisFixture;

  [Fact]
  public async Task CreateProduct_ShouldCreateProductNInsertIntoDB()
  {
    //Arrange
    var product = new Product()
    {
      Id = 200,
      Name = "PcMsi",
      Price = 1300,
      Description = "NasaPc",
      Stock = 5
    };
    //Act
    await CreateProduct.AddProduct(product, _postgresFixure._dbContext);
    //Assert
    var productInDB = await _postgresFixure._dbContext.Products.FirstOrDefaultAsync(x => x.Id == 200);
    Assert.IsType<Product>(productInDB);
    Assert.Equal("NasaPc", productInDB.Description);
  }
  
  [Theory]
  [InlineData(2,true,"database")]
  [InlineData(2,true,"cache")]
  [InlineData(900,false,"_blank")]
  public async Task RetrieveProductById_ShouldReturnFromCacheOrDbAndUpdateCache(int id, bool isFounded,string source)
  {
    var connection = StackExchange.Redis.ConnectionMultiplexer.Connect(this._redisFixure.ConnectionString);
    var redisCache = connection.GetDatabase();
    var redisOpt = new RedisCacheOptions()
    {
      Configuration = _redisFixure.ConnectionString
    };
    IDistributedCache distCache = new RedisCache(redisOpt);


    var result = await GetProductById.Execute(id,_redisFixure._cacheSettings ,distCache,_postgresFixure._dbContext);
    if (isFounded)
    {
    Assert.NotNull(result);
    Assert.Equal(source,result.source);
    Assert.True(result.isFound);  
    }
    else
    {
    Assert.NotNull(result);
    Assert.Equal(source,result.source);
    Assert.False(result.isFound);  
    }
    

  }

}
