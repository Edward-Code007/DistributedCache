using Testcontainers.Redis;

namespace DistributedCache.test;

public class IntegrationTest
{
    public RedisConfiguration _redisConfig{get;set;}
    public RedisContainer _redisContainer{get;set;}
    public IntegrationTest()
    {
        this._redisConfig = new RedisConfiguration();
        this._redisContainer = new RedisBuilder("redis:latest")
        .WithCleanUp(true)
        .WithAutoRemove(true)
        .Build();
        this._redisContainer.StartAsync();

    }
    [Fact]
    public async Task CreateProduct_ShouldCreateProductNInsertIntoDB()
    {
        

    }
}
