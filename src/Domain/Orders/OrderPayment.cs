namespace Domain.Orders;

public sealed class OrderPayment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }

    public Order Order { get; set; } = null!;
}
