using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Orders;

internal sealed class OrderPaymentConfiguration : IEntityTypeConfiguration<OrderPayment>
{
    public void Configure(EntityTypeBuilder<OrderPayment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasPrecision(18, 4);
    }
}
