using SharedKernel;

namespace Domain.Products;

/// <summary>
/// Default price: BranchId = null. Branch override: BranchId set.
/// </summary>
public sealed class ProductPrice : Entity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? BranchId { get; set; }
    public decimal Price { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public Product Product { get; set; } = null!;
    public Branch? Branch { get; set; }
}
