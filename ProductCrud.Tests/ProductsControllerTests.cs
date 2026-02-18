using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using ProductCrud.Web.Controllers;
using ProductCrud.Web.Models;
using ProductCrud.Web.Services;

namespace ProductCrud.Tests;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _serviceMock = new Mock<IProductService>();
        var logger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_serviceMock.Object, logger.Object);

        // Setup TempData
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    [Fact]
    public async Task Index_ReturnsViewWithProducts()
    {
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Test", Category = "Cat1", Price = 100 }
        };
        _serviceMock.Setup(s => s.GetProductsAsync(null, null, "Name", false, 1, 10))
            .ReturnsAsync((products, 1));
        _serviceMock.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(new List<string> { "Cat1" });

        var result = await _controller.Index(null, null, "Name", false, 1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProductListViewModel>(viewResult.Model);
        Assert.Single(model.Products);
        Assert.Equal(1, model.TotalItems);
    }

    [Fact]
    public async Task Index_PassesSearchParams()
    {
        _serviceMock.Setup(s => s.GetProductsAsync("test", "Electronics", "Price", true, 2, 10))
            .ReturnsAsync((new List<Product>(), 0));
        _serviceMock.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(new List<string>());

        var result = await _controller.Index("test", "Electronics", "Price", true, 2);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProductListViewModel>(viewResult.Model);
        Assert.Equal("test", model.SearchQuery);
        Assert.Equal("Electronics", model.CategoryFilter);
        Assert.Equal("Price", model.SortBy);
        Assert.True(model.SortDescending);
        Assert.Equal(2, model.Page);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        var result = _controller.Create();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<Product>(viewResult.Model);
    }

    [Fact]
    public async Task Create_Post_ValidModel_RedirectsToIndex()
    {
        var product = new Product { Name = "New Product", Category = "Cat1", Price = 100 };
        _serviceMock.Setup(s => s.CreateAsync(product)).ReturnsAsync(product);

        var result = await _controller.Create(product);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        _serviceMock.Verify(s => s.CreateAsync(product), Times.Once);
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var product = new Product();

        var result = await _controller.Create(product);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<Product>(viewResult.Model);
        _serviceMock.Verify(s => s.CreateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Edit_Get_ExistingProduct_ReturnsView()
    {
        var id = Guid.NewGuid();
        var product = new Product { Id = id, Name = "Test", Category = "Cat1", Price = 100 };
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(product);

        var result = await _controller.Edit(id);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Product>(viewResult.Model);
        Assert.Equal(id, model.Id);
    }

    [Fact]
    public async Task Edit_Get_NonExistent_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await _controller.Edit(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_RedirectsToIndex()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Updated", Category = "Cat1", Price = 200 };
        _serviceMock.Setup(s => s.UpdateAsync(product)).ReturnsAsync(product);

        var result = await _controller.Edit(product);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var product = new Product();

        var result = await _controller.Edit(product);

        var viewResult = Assert.IsType<ViewResult>(result);
        _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Edit_Post_NotFound_ReturnsNotFound()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Test", Category = "Cat1", Price = 100 };
        _serviceMock.Setup(s => s.UpdateAsync(product)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Edit(product);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ExistingProduct_RedirectsToIndex()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

        var result = await _controller.Delete(id);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Post_SetsTempDataSuccess()
    {
        var product = new Product { Name = "New", Category = "Cat1", Price = 50 };
        _serviceMock.Setup(s => s.CreateAsync(product)).ReturnsAsync(product);

        await _controller.Create(product);

        Assert.Equal("Товар успешно создан", _controller.TempData["Success"]);
    }

    [Fact]
    public async Task Edit_Post_SetsTempDataSuccess()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Updated", Category = "Cat1", Price = 200 };
        _serviceMock.Setup(s => s.UpdateAsync(product)).ReturnsAsync(product);

        await _controller.Edit(product);

        Assert.Equal("Товар успешно обновлён", _controller.TempData["Success"]);
    }

    [Fact]
    public async Task Delete_SetsTempDataSuccess()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

        await _controller.Delete(id);

        Assert.Equal("Товар удалён", _controller.TempData["Success"]);
    }
}
