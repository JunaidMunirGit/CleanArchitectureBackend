using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

internal sealed class TaxCategoryConfiguration : IEntityTypeConfiguration<TaxCategory>
{
    public void Configure(EntityTypeBuilder<TaxCategory> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasQueryFilter(t => !t.IsDeleted);
        builder.Property(t => t.Code).HasMaxLength(20);
        builder.Property(t => t.Name).HasMaxLength(100);
        builder.Property(t => t.RatePercent).HasPrecision(5, 2);
    }
}
