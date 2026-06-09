using DistributedCache.Data;
using DistributedCache.Features.Products;
using DistributedCache.Models;
using DistributedCache.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

public class ProductsUnitTest
{
    private readonly AppDbContext _appDbContext;
    public ProductsUnitTest()
    {
        var dbOpt = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var dbSettingsMock = new Mock<IOptions<DatabaseSettings>>();
        var iHostEnvMock = new Mock<IHostEnvironment>();
        iHostEnvMock.SetupGet(x => x.EnvironmentName).Returns("Testing");
        var dbContext = new AppDbContext(dbOpt, dbSettingsMock.Object, iHostEnvMock.Object);
        dbContext.Database.EnsureCreated();
        this._appDbContext = dbContext;
    }

    [Fact]
    public async Task GetAllProduct_ShouldReturnAllProducts()
    {
        // Arrange
        _appDbContext.Products.AddRange(
            new Product { Id = 1, Name = "Laptop", Price = 1299.99m, Stock = 15 },
            new Product { Id = 2, Name = "Mouse", Price = 29.99m, Stock = 200 }
        );
        await _appDbContext.SaveChangesAsync();

        // Act
        var result = await GetAllProducts.RetrieveAllProducts(_appDbContext);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.IsType<List<Product>>(result);
    }
}
