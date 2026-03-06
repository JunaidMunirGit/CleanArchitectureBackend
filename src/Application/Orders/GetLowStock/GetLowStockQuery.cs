using Application.Abstractions.Messaging;

namespace Application.Orders.GetLowStock;

public sealed class GetLowStockQuery : IQuery<List<LowStockItemDto>>;

public sealed class LowStockItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public decimal ReorderLevel { get; set; }
}
