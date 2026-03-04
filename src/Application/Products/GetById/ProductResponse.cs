namespace Application.Products.GetById;

public sealed class ProductResponse
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public string? UnitOfMeasureName { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? EffectivePrice { get; set; }
    public bool Discountable { get; set; }
    public bool TrackInventory { get; set; }
    public decimal ReorderLevel { get; set; }
    public Guid TaxCategoryId { get; set; }
    public string? TaxCategoryName { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public List<string> Barcodes { get; set; } = [];
    public string? PrimaryBarcode { get; set; }
}
