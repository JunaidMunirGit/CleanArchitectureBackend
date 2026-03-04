using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public static class ProductSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        DateTime utc = DateTime.UtcNow;
        var cat1 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "General Merchandise",
            IsActive = true,
            CreatedAt = utc,
            UpdatedAt = utc
        };
        var cat2 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Food & Beverage",
            IsActive = true,
            CreatedAt = utc,
            UpdatedAt = utc
        };

        context.Categories.AddRange(cat1, cat2);

        var uomEach = new UnitOfMeasure { Id = Guid.NewGuid(), Code = "EACH", Name = "Each" };
        var uomKg = new UnitOfMeasure { Id = Guid.NewGuid(), Code = "KG", Name = "Kilogram" };
        context.UnitOfMeasures.AddRange(uomEach, uomKg);

        var taxZero = new TaxCategory { Id = Guid.NewGuid(), Code = "ZERO", Name = "Zero Rate", RatePercent = 0 };
        var taxStandard = new TaxCategory { Id = Guid.NewGuid(), Code = "STD", Name = "Standard", RatePercent = 10 };
        context.TaxCategories.AddRange(taxZero, taxStandard);

        var brand1 = new Brand { Id = Guid.NewGuid(), Name = "Acme", IsActive = true, CreatedAt = utc, UpdatedAt = utc };
        context.Brands.Add(brand1);

        await context.SaveChangesAsync(cancellationToken);

        for (int i = 1; i <= 5; i++)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Sku = $"SKU-{i:D4}",
                Name = $"Product {i}",
                Description = $"Description for product {i}",
                CategoryId = i % 2 == 0 ? cat2.Id : cat1.Id,
                BrandId = brand1.Id,
                UnitOfMeasureId = uomEach.Id,
                CostPrice = 10 * i,
                SellingPrice = 15 * i,
                Discountable = true,
                TrackInventory = true,
                ReorderLevel = 5,
                TaxCategoryId = taxStandard.Id,
                IsActive = true,
                CreatedAt = utc,
                UpdatedAt = utc,
                RowVersion = [1, 0, 0, 0, 0, 0, 0, 0]
            };
            product.Barcodes.Add(new ProductBarcode { Barcode = $"590123412345{i}", IsPrimary = true });
            context.Products.Add(product);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
