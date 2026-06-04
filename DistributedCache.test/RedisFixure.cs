using System.Threading.Tasks;
using DistributedCache.Settings;
using Testcontainers.Redis;
using Xunit;

public class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container;
    public RedisSettings _redisSettings;
    public CacheSettings _cacheSettings;

    public string ConnectionString => $"{_container.GetConnectionString()}";

    public RedisFixture()
    {
        _redisSettings = new RedisSettings()
        {
            Hostname="localhost",
            Port="6379",
            InstanceName="redisTest"
        };
        _cacheSettings = new CacheSettings();

        _container = new RedisBuilder("redis:7-alpine") 
            .WithPortBinding(int.Parse(_redisSettings.Port),6379)
            .WithHostname(_redisSettings.Hostname)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
