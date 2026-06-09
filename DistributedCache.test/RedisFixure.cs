using DistributedCache.Settings;
using Testcontainers.Redis;

public class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container;
    public CacheSettings _cacheSettings;

    public string ConnectionString => _container.GetConnectionString();

    public RedisFixture()
    {
        _cacheSettings = new CacheSettings();
        _container = new RedisBuilder("redis:7-alpine").Build();
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
