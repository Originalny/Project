using ProductCrud.Web.Models;

namespace ProductCrud.Web.Services;

public interface IProductService
{
    Task<(List<Product> Items, int TotalCount)> GetProductsAsync(
        string? search, string? category, string sortBy, bool sortDesc, int page, int pageSize);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(Guid id);
    Task<List<string>> GetCategoriesAsync();
}
