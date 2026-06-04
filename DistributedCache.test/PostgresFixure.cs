using DistributedCache.Data;
using DistributedCache.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Testcontainers.PostgreSql;
public class PostgresFixture : IAsyncLifetime
{
    public readonly PostgreSqlContainer _container;
    public AppDbContext _dbContext;
    public readonly string _stringConnection;
    public DatabaseSettings _dbSettings;
    public PostgresFixture()
    {
        _dbSettings = new DatabaseSettings()
        {
            DbName = "dbtest",
            Hostname = "localhost",
            User = "user",
            Password = "password",
            Port = "5432",
        };
        this._container = new PostgreSqlBuilder("postgres")
        .WithDatabase(_dbSettings.DbName)
        .WithUsername(_dbSettings.User)
        .WithPassword(_dbSettings.Password)
        .WithHostname(_dbSettings.Hostname)
        .WithPortBinding(int.Parse(_dbSettings.Port), 5432)
        .Build();
    }
    public async Task DisposeAsync()
    {
        await this._container.DisposeAsync();

    }

    public async Task InitializeAsync()
    {
        await this._container.StartAsync();
        IOptions<DatabaseSettings> dbSettingsOpt = Options.Create(_dbSettings);
        var hostEnvMock = new Mock<IHostEnvironment>();
        var dbContextOptMock = new DbContextOptions<AppDbContext>();
        hostEnvMock.SetupGet(x => x.EnvironmentName).Returns("Development");
        _dbContext = new AppDbContext(dbContextOptMock, dbSettingsOpt, hostEnvMock.Object);
        await _dbContext.Database.MigrateAsync();
    }
}