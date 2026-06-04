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
        .UseInMemoryDatabase("dbTets").Options;

        var dbSettingsMock = new Mock<IOptions<DatabaseSettings>>();

        var iHostEnvMock = new Mock<IHostEnvironment>();

        iHostEnvMock.SetupGet(x => x.EnvironmentName).Returns("xUnit");
        var dbContext = new AppDbContext(dbOpt, dbSettingsMock.Object, iHostEnvMock.Object);
        dbContext.Database.EnsureCreated();
        this._appDbContext = dbContext;
    }
    [Fact]
    public async Task GetAllProduct_ShouldReturnAllProducts()
    {
        // Arrange
        
        // Act
        var result = await GetAllProducts.RetrieveAllProducts(_appDbContext);
        // Assert

        Assert.NotEmpty(result);
        Assert.IsType<List<Product>>(result);

    }


}