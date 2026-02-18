using System.ComponentModel.DataAnnotations;

namespace ProductCrud.Web.Models;

public class Product
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 50 символов")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Описание не должно превышать 255 символов")]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Display(Name = "Цена")]
    [Range(0.01, 999999.99, ErrorMessage = "Цена должна быть от 0.01 до 999999.99")]
    [Required(ErrorMessage = "Цена обязательна")]
    public decimal Price { get; set; }

    [Display(Name = "Категория")]
    [Required(ErrorMessage = "Категория обязательна")]
    [StringLength(50, ErrorMessage = "Категория не должна превышать 50 символов")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Дата создания")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Дата обновления")]
    public DateTime UpdatedAt { get; set; }
}
