using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

internal sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasQueryFilter(b => !b.IsDeleted);
        builder.Property(b => b.Name).HasMaxLength(200);
        builder.Property(b => b.Code).HasMaxLength(20);
    }
}
