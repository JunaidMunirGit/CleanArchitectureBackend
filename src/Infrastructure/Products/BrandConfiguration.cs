using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

internal sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasQueryFilter(b => !b.IsDeleted);
        builder.Property(b => b.Name).HasMaxLength(200);
        builder.Property(b => b.Description).HasMaxLength(500);
    }
}
