using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductCrud.Web.Data;
using ProductCrud.Web.Models;

namespace ProductCrud.Web.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProductService> _logger;

    public ProductService(AppDbContext db, ILogger<ProductService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(List<Product> Items, int TotalCount)> GetProductsAsync(
        string? search, string? category, string sortBy, bool sortDesc, int page, int pageSize)
    {
        _logger.LogInformation("Getting products: search={Search}, category={Category}, sort={Sort}, page={Page}",
            search, category, sortBy, page);

        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) ||
                                     (p.Description != null && p.Description.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        query = sortBy switch
        {
            "Name" => sortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "Price" => sortDesc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "Category" => sortDesc ? query.OrderByDescending(p => p.Category) : query.OrderBy(p => p.Category),
            "CreatedAt" => sortDesc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        _logger.LogInformation("Found {Count} products (total: {Total})", items.Count, total);
        return (items, total);
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting product by id: {Id}", id);
        return await _db.Products.FindAsync(id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created product: {Id} - {Name}", product.Id, product.Name);
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        var existing = await _db.Products.FindAsync(product.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Product {product.Id} not found");

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Category = product.Category;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated product: {Id} - {Name}", existing.Id, existing.Name);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found for deletion: {Id}", id);
            return false;
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted product: {Id} - {Name}", id, product.Name);
        return true;
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        return await _db.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync();
    }
}
