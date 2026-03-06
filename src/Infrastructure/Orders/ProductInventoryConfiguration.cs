using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Orders;

internal sealed class ProductInventoryConfiguration : IEntityTypeConfiguration<ProductInventory>
{
    public void Configure(EntityTypeBuilder<ProductInventory> builder)
    {
        builder.HasKey(p => p.ProductId);
    }
}
