using Application.Abstractions.Messaging;

namespace Application.Products.BulkImport;

public sealed class BulkImportProductsCommand : ICommand<BulkImportProductsResult>
{
    public List<BulkImportProductItem> Items { get; set; } = [];
    public bool AllOrNothing { get; set; }
}

public sealed class BulkImportProductItem
{
    public string? IdempotencyKey { get; set; }
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
    public string? ImageUrl { get; set; }
    public List<string> Barcodes { get; set; } = [];
    public string? PrimaryBarcode { get; set; }
}
