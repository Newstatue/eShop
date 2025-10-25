namespace Catalog.Data;

public static class Extensions
{
    public static void UseMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

        context.Database.Migrate();
        DataSeeder.Seed(context);
    }
}

public class DataSeeder
{
    public static void Seed(ProductDbContext dbContext)
    {
        if (dbContext.Products.Any())
        {
            return; // DB has been seeded
        }

        dbContext.Products.AddRange(Products);
        dbContext.SaveChanges();
    }

    public static IEnumerable<Product> Products =>
    [
        new Product { Name = "太阳能手电筒", Description = "户外爱好者的绝佳产品", Price = 19.99m, ImageUrl = "product1.png" },
        new Product { Name = "登山杖", Description = "适合露营和徒步旅行", Price = 24.99m, ImageUrl = "product2.png" },
        new Product { Name = "户外雨衣", Description = "让你在各种天气下保持温暖干爽", Price = 49.99m, ImageUrl = "product3.png" },
        new Product { Name = "生存套件", Description = "任何户外探险者的必备装备", Price = 99.99m, ImageUrl = "product4.png" },
        new Product { Name = "户外背包", Description = "完美携带所有户外必需品的背包", Price = 39.99m, ImageUrl = "product5.png" },
        new Product { Name = "露营炊具", Description = "理想的户外烹饪炊具套装", Price = 29.99m, ImageUrl = "product6.png" },
        new Product { Name = "露营炉", Description = "户外烹饪的完美选择", Price = 49.99m, ImageUrl = "product7.png" },
        new Product { Name = "露营灯", Description = "照亮营地的完美灯具", Price = 19.99m, ImageUrl = "product8.png" },
        new Product { Name = "露营帐篷", Description = "适合露营旅行的帐篷", Price = 99.99m, ImageUrl = "product9.png" }
    ];
}