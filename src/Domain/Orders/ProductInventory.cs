namespace Domain.Orders;

public sealed class ProductInventory
{
    public Guid ProductId { get; set; }
    public int QuantityOnHand { get; set; }
    public DateTime UpdatedAt { get; set; }
}
