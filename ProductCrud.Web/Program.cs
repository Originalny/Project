using Microsoft.EntityFrameworkCore;
using ProductCrud.Web.Data;
using ProductCrud.Web.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=products.db"));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new ProductCrud.Web.Models.Product { Id = Guid.NewGuid(), Name = "Ноутбук", Description = "Игровой ноутбук 15.6\"", Price = 89990, Category = "Электроника", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new ProductCrud.Web.Models.Product { Id = Guid.NewGuid(), Name = "Наушники", Description = "Беспроводные наушники с шумоподавлением", Price = 12990, Category = "Электроника", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new ProductCrud.Web.Models.Product { Id = Guid.NewGuid(), Name = "Футболка", Description = "Хлопковая футболка", Price = 1990, Category = "Одежда", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new ProductCrud.Web.Models.Product { Id = Guid.NewGuid(), Name = "Кроссовки", Description = "Спортивные кроссовки", Price = 7490, Category = "Обувь", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new ProductCrud.Web.Models.Product { Id = Guid.NewGuid(), Name = "Рюкзак", Description = "Городской рюкзак 30л", Price = 3490, Category = "Аксессуары", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}")
    .WithStaticAssets();

Log.Information("Application started");
app.Run();
