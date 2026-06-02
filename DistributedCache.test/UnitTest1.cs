using Testcontainers.Redis;

namespace DistributedCache.test;

public class IntegrationTest
{
    [Fact]
    public async Task CreateProduct_ShouldCreateProductNInsertIntoDB()
    {
        var redisConfig = new RedisConfiguration();
       var redisContainer = new RedisBuilder("redis:latest").Build();
      await redisContainer.StartAsync();
      redisContainer.
    }
}
