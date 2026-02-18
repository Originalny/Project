namespace ProductCrud.Web.Models;

public class ProductListViewModel
{
    public List<Product> Products { get; set; } = new();
    public string? SearchQuery { get; set; }
    public string? CategoryFilter { get; set; }
    public string SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public List<string> Categories { get; set; } = new();
}
