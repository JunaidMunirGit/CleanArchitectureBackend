using SharedKernel;

namespace Domain.Orders;

public sealed class Order : Entity
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<OrderLine> Lines { get; set; } = [];
    public ICollection<OrderPayment> Payments { get; set; } = [];
}

public enum OrderStatus
{
    Pending = 0,
    Completed = 1,
    Cancelled = 2
}
