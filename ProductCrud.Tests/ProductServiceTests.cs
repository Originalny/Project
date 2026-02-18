using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProductCrud.Web.Data;
using ProductCrud.Web.Models;
using ProductCrud.Web.Services;

namespace ProductCrud.Tests;

public class ProductServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        var logger = new Mock<ILogger<ProductService>>();
        _service = new ProductService(_db, logger.Object);
    }

    public void Dispose() => _db.Dispose();

    private Product MakeProduct(string name = "Test", string category = "Cat1", decimal price = 100)
    {
        return new Product
        {
            Name = name,
            Description = "Desc",
            Price = price,
            Category = category
        };
    }

    [Fact]
    public async Task Create_SetsIdAndDates()
    {
        var p = MakeProduct();
        var result = await _service.CreateAsync(p);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.CreatedAt > DateTime.MinValue);
        Assert.True(result.UpdatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task Create_SavesInDb()
    {
        await _service.CreateAsync(MakeProduct("Laptop"));
        Assert.Single(_db.Products);
        Assert.Equal("Laptop", _db.Products.First().Name);
    }

    [Fact]
    public async Task GetById_ReturnsProduct()
    {
        var created = await _service.CreateAsync(MakeProduct());
        var found = await _service.GetByIdAsync(created.Id);
        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNullForMissing()
    {
        var found = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(found);
    }

    [Fact]
    public async Task Update_ChangesFields()
    {
        var created = await _service.CreateAsync(MakeProduct("Old"));
        created.Name = "New";
        created.Price = 200;
        var updated = await _service.UpdateAsync(created);

        Assert.Equal("New", updated.Name);
        Assert.Equal(200, updated.Price);
        Assert.True(updated.UpdatedAt >= updated.CreatedAt);
    }

    [Fact]
    public async Task Update_ThrowsForMissing()
    {
        var p = MakeProduct();
        p.Id = Guid.NewGuid();
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(p));
    }

    [Fact]
    public async Task Delete_RemovesProduct()
    {
        var created = await _service.CreateAsync(MakeProduct());
        var result = await _service.DeleteAsync(created.Id);

        Assert.True(result);
        Assert.Empty(_db.Products);
    }

    [Fact]
    public async Task Delete_ReturnsFalseForMissing()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid());
        Assert.False(result);
    }

    [Fact]
    public async Task GetProducts_ReturnsList()
    {
        await _service.CreateAsync(MakeProduct("A"));
        await _service.CreateAsync(MakeProduct("B"));
        await _service.CreateAsync(MakeProduct("C"));

        var (items, total) = await _service.GetProductsAsync(null, null, "Name", false, 1, 10);
        Assert.Equal(3, total);
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public async Task GetProducts_SearchByName()
    {
        await _service.CreateAsync(MakeProduct("Laptop"));
        await _service.CreateAsync(MakeProduct("Phone"));

        var (items, total) = await _service.GetProductsAsync("Lap", null, "Name", false, 1, 10);
        Assert.Single(items);
        Assert.Equal("Laptop", items[0].Name);
    }

    [Fact]
    public async Task GetProducts_SearchByDescription()
    {
        var p = MakeProduct("Item");
        p.Description = "Special description here";
        await _service.CreateAsync(p);
        await _service.CreateAsync(MakeProduct("Other"));

        var (items, _) = await _service.GetProductsAsync("Special", null, "Name", false, 1, 10);
        Assert.Single(items);
    }

    [Fact]
    public async Task GetProducts_FilterByCategory()
    {
        await _service.CreateAsync(MakeProduct("A", "Electronics"));
        await _service.CreateAsync(MakeProduct("B", "Clothes"));

        var (items, _) = await _service.GetProductsAsync(null, "Electronics", "Name", false, 1, 10);
        Assert.Single(items);
        Assert.Equal("Electronics", items[0].Category);
    }

    [Fact]
    public async Task GetProducts_SortByNameAsc()
    {
        await _service.CreateAsync(MakeProduct("Banana"));
        await _service.CreateAsync(MakeProduct("Apple"));

        var (items, _) = await _service.GetProductsAsync(null, null, "Name", false, 1, 10);
        Assert.Equal("Apple", items[0].Name);
        Assert.Equal("Banana", items[1].Name);
    }

    [Fact]
    public async Task GetProducts_SortByNameDesc()
    {
        await _service.CreateAsync(MakeProduct("Apple"));
        await _service.CreateAsync(MakeProduct("Banana"));

        var (items, _) = await _service.GetProductsAsync(null, null, "Name", true, 1, 10);
        Assert.Equal("Banana", items[0].Name);
    }

    [Fact]
    public async Task GetProducts_SortByPrice()
    {
        await _service.CreateAsync(MakeProduct("Cheap", price: 10));
        await _service.CreateAsync(MakeProduct("Expensive", price: 1000));

        var (items, _) = await _service.GetProductsAsync(null, null, "Price", false, 1, 10);
        Assert.Equal(10, items[0].Price);
    }

    [Fact]
    public async Task GetProducts_SortByCategory()
    {
        await _service.CreateAsync(MakeProduct("A", "Zzz"));
        await _service.CreateAsync(MakeProduct("B", "Aaa"));

        var (items, _) = await _service.GetProductsAsync(null, null, "Category", false, 1, 10);
        Assert.Equal("Aaa", items[0].Category);
    }

    [Fact]
    public async Task GetProducts_SortByCreatedAt()
    {
        await _service.CreateAsync(MakeProduct("First"));
        await Task.Delay(10);
        await _service.CreateAsync(MakeProduct("Second"));

        var (items, _) = await _service.GetProductsAsync(null, null, "CreatedAt", false, 1, 10);
        Assert.Equal("First", items[0].Name);
    }

    [Fact]
    public async Task GetProducts_Pagination()
    {
        for (int i = 0; i < 15; i++)
            await _service.CreateAsync(MakeProduct($"Item{i:D2}"));

        var (page1, total) = await _service.GetProductsAsync(null, null, "Name", false, 1, 5);
        Assert.Equal(15, total);
        Assert.Equal(5, page1.Count);

        var (page2, _) = await _service.GetProductsAsync(null, null, "Name", false, 2, 5);
        Assert.Equal(5, page2.Count);
        Assert.NotEqual(page1[0].Name, page2[0].Name);

        var (page3, _) = await _service.GetProductsAsync(null, null, "Name", false, 3, 5);
        Assert.Equal(5, page3.Count);
    }

    [Fact]
    public async Task GetProducts_EmptyPageReturnsEmpty()
    {
        await _service.CreateAsync(MakeProduct("A"));
        var (items, total) = await _service.GetProductsAsync(null, null, "Name", false, 100, 10);
        Assert.Empty(items);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task GetCategories_ReturnsDistinct()
    {
        await _service.CreateAsync(MakeProduct("A", "Cat1"));
        await _service.CreateAsync(MakeProduct("B", "Cat1"));
        await _service.CreateAsync(MakeProduct("C", "Cat2"));

        var cats = await _service.GetCategoriesAsync();
        Assert.Equal(2, cats.Count);
        Assert.Contains("Cat1", cats);
        Assert.Contains("Cat2", cats);
    }

    [Fact]
    public async Task GetCategories_ReturnsSorted()
    {
        await _service.CreateAsync(MakeProduct("A", "Zzz"));
        await _service.CreateAsync(MakeProduct("B", "Aaa"));

        var cats = await _service.GetCategoriesAsync();
        Assert.Equal("Aaa", cats[0]);
        Assert.Equal("Zzz", cats[1]);
    }

    [Fact]
    public async Task GetProducts_DefaultSortForUnknown()
    {
        await _service.CreateAsync(MakeProduct("Banana"));
        await _service.CreateAsync(MakeProduct("Apple"));

        var (items, _) = await _service.GetProductsAsync(null, null, "Unknown", false, 1, 10);
        Assert.Equal("Apple", items[0].Name);
    }

    [Fact]
    public async Task Update_PreservesCreatedAt()
    {
        var created = await _service.CreateAsync(MakeProduct());
        var originalCreatedAt = created.CreatedAt;

        await Task.Delay(10);
        created.Name = "Updated";
        var updated = await _service.UpdateAsync(created);

        Assert.Equal(originalCreatedAt, updated.CreatedAt);
    }

    [Fact]
    public async Task GetProducts_SearchCaseInsensitive()
    {
        await _service.CreateAsync(MakeProduct("LAPTOP"));

        var (items, _) = await _service.GetProductsAsync("laptop", null, "Name", false, 1, 10);
        Assert.Single(items);
    }

    [Fact]
    public async Task GetProducts_CombineSearchAndCategory()
    {
        await _service.CreateAsync(MakeProduct("Laptop", "Electronics"));
        await _service.CreateAsync(MakeProduct("Laptop Case", "Accessories"));

        var (items, _) = await _service.GetProductsAsync("Laptop", "Electronics", "Name", false, 1, 10);
        Assert.Single(items);
        Assert.Equal("Electronics", items[0].Category);
    }
}
