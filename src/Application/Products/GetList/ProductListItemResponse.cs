namespace Application.Products.GetList;

public sealed class ProductListItemResponse
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? EffectivePrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? PrimaryBarcode { get; set; }
}
