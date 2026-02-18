using Microsoft.AspNetCore.Mvc;
using ProductCrud.Web.Models;
using ProductCrud.Web.Services;

namespace ProductCrud.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService service, ILogger<ProductsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? search, string? category,
        string sortBy = "Name", bool sortDesc = false, int page = 1)
    {
        var (items, total) = await _service.GetProductsAsync(search, category, sortBy, sortDesc, page, 10);
        var categories = await _service.GetCategoriesAsync();

        var vm = new ProductListViewModel
        {
            Products = items,
            SearchQuery = search,
            CategoryFilter = category,
            SortBy = sortBy,
            SortDescending = sortDesc,
            Page = page,
            PageSize = 10,
            TotalItems = total,
            Categories = categories
        };

        return View(vm);
    }

    public IActionResult Create()
    {
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
            return View(product);

        await _service.CreateAsync(product);
        TempData["Success"] = "Товар успешно создан";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product product)
    {
        if (!ModelState.IsValid)
            return View(product);

        try
        {
            await _service.UpdateAsync(product);
            TempData["Success"] = "Товар успешно обновлён";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound();

        TempData["Success"] = "Товар удалён";
        return RedirectToAction(nameof(Index));
    }
}
