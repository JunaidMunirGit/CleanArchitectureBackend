namespace Application.Products.GetPrices;

public sealed class ProductPriceResponse
{
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public decimal Price { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
