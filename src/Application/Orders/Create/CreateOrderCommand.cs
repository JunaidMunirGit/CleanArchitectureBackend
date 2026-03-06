using Application.Abstractions.Messaging;
using Domain.Orders;

namespace Application.Orders.Create;

public sealed record CreateOrderLineRequest(Guid ProductId, int Quantity);

public sealed record CreateOrderPaymentRequest(PaymentMethod Method, decimal Amount);

public sealed class CreateOrderCommand : ICommand<Guid>
{
    public IReadOnlyList<CreateOrderLineRequest> Lines { get; set; } = [];
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; } = 10m;
    public PaymentMethod PaymentMethod { get; set; }
    /// <summary>Required when PaymentMethod is Split. Sum must equal order total.</summary>
    public IReadOnlyList<CreateOrderPaymentRequest>? SplitPayments { get; set; }
}
