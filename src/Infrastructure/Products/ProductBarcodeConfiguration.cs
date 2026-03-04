using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

internal sealed class ProductBarcodeConfiguration : IEntityTypeConfiguration<ProductBarcode>
{
    public void Configure(EntityTypeBuilder<ProductBarcode> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasQueryFilter(b => !b.IsDeleted);
        builder.Property(b => b.Barcode).HasMaxLength(100);

        builder.HasIndex(b => b.Barcode).IsUnique().HasFilter("[is_deleted] = 0");
        builder.HasIndex(b => b.ProductId);
    }
}
