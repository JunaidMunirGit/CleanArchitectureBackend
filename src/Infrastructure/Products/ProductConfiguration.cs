using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.Property(p => p.RowVersion).IsRowVersion();
        builder.Property(p => p.Sku).HasMaxLength(50);
        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.ImageUrl).HasMaxLength(500);
        builder.Property(p => p.CostPrice).HasPrecision(18, 4);
        builder.Property(p => p.SellingPrice).HasPrecision(18, 4);
        builder.Property(p => p.ReorderLevel).HasPrecision(18, 4);

        builder.HasIndex(p => p.Sku).IsUnique().HasFilter("[is_deleted] = 0");
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => new { p.CategoryId, p.IsDeleted });
        builder.HasIndex(p => p.IsActive);

        builder.HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId);
        builder.HasOne(p => p.Brand).WithMany().HasForeignKey(p => p.BrandId).IsRequired(false);
        builder.HasOne(p => p.UnitOfMeasure).WithMany().HasForeignKey(p => p.UnitOfMeasureId);
        builder.HasOne(p => p.TaxCategory).WithMany().HasForeignKey(p => p.TaxCategoryId);
        builder.HasMany(p => p.Barcodes).WithOne(b => b.Product).HasForeignKey(b => b.ProductId);
        builder.HasMany(p => p.Prices).WithOne(pr => pr.Product).HasForeignKey(pr => pr.ProductId);
    }
}
