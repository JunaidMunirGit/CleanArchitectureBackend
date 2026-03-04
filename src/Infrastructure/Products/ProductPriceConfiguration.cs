using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

internal sealed class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.HasKey(p => p.Id);

        // Match Product's soft-delete filter so prices for deleted products are excluded.
        builder.HasQueryFilter(pp => !pp.Product.IsDeleted);

        builder.Property(p => p.Price).HasPrecision(18, 4);
        builder.HasIndex(p => new { p.ProductId, p.BranchId });
        builder.HasOne(p => p.Branch).WithMany().HasForeignKey(p => p.BranchId).IsRequired(false);
    }
}
