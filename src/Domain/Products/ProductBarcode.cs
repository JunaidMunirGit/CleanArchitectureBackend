using SharedKernel;

namespace Domain.Products;

public sealed class ProductBarcode : Entity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsDeleted { get; set; }

    public Product Product { get; set; } = null!;
}
