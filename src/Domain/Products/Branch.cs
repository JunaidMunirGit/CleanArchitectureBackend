using SharedKernel;

namespace Domain.Products;

public sealed class Branch : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}
