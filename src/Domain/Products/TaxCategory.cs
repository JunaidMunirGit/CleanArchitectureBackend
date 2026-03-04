using SharedKernel;

namespace Domain.Products;

public sealed class TaxCategory : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal RatePercent { get; set; }
    public bool IsDeleted { get; set; }
}
