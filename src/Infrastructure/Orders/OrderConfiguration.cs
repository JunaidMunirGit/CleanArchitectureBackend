using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Orders;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNumber).HasMaxLength(32).IsRequired();
        builder.Property(o => o.SubTotal).HasPrecision(18, 4);
        builder.Property(o => o.TaxAmount).HasPrecision(18, 4);
        builder.Property(o => o.DiscountAmount).HasPrecision(18, 4);
        builder.Property(o => o.Total).HasPrecision(18, 4);
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => new { o.CreatedAt, o.Status });
        builder.HasMany(o => o.Lines).WithOne(l => l.Order).HasForeignKey(l => l.OrderId);
        builder.HasMany(o => o.Payments).WithOne(p => p.Order).HasForeignKey(p => p.OrderId);
    }
}
