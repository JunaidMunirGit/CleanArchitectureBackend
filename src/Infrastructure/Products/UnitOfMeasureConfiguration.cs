using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

internal sealed class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasQueryFilter(u => !u.IsDeleted);
        builder.Property(u => u.Code).HasMaxLength(20);
        builder.Property(u => u.Name).HasMaxLength(100);
    }
}
