using SharedKernel;

namespace Domain.Products;

public sealed class Product : Entity
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public bool Discountable { get; set; }
    public bool TrackInventory { get; set; }
    public decimal ReorderLevel { get; set; }
    public Guid TaxCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public bool IsDeleted { get; set; }

    public Category Category { get; set; } = null!;
    public Brand? Brand { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    public TaxCategory TaxCategory { get; set; } = null!;
    public ICollection<ProductBarcode> Barcodes { get; set; } = [];
    public ICollection<ProductPrice> Prices { get; set; } = [];
}
