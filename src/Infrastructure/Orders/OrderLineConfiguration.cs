using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Orders;

internal sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.ProductName).HasMaxLength(200);
        builder.Property(l => l.UnitPrice).HasPrecision(18, 4);
        builder.Property(l => l.LineTotal).HasPrecision(18, 4);
    }
}
