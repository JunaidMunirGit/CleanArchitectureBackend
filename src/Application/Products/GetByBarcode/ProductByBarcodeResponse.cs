namespace Application.Products.GetByBarcode;

public sealed class ProductByBarcodeResponse
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public decimal? EffectivePrice { get; set; }
    public bool IsActive { get; set; }
}
