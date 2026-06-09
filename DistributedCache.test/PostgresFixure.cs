using DistributedCache.Data;
using DistributedCache.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Testcontainers.PostgreSql;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    public AppDbContext _dbContext = null!;

    public PostgresFixture()
    {
        _container = new PostgreSqlBuilder("postgres").Build();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _container.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var dbContextOpts = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        var hostEnvMock = new Mock<IHostEnvironment>();
        hostEnvMock.SetupGet(x => x.EnvironmentName).Returns("Development");

        _dbContext = new AppDbContext(dbContextOpts, Options.Create(new DatabaseSettings()), hostEnvMock.Object);
        await _dbContext.Database.MigrateAsync();
    }
}
