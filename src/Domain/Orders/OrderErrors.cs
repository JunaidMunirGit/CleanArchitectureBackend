using SharedKernel;

namespace Domain.Orders;

public static class OrderErrors
{
    public static Error ProductNotFound(Guid id) =>
        Error.NotFound("Order.ProductNotFound", $"Product with id '{id}' was not found.");

    public static Error InsufficientStock(Guid productId, string productName, int requested, int available) =>
        Error.Validation("Order.InsufficientStock",
            $"Insufficient stock for '{productName}': requested {requested}, available {available}.");

    public static Error InvalidOrderLines =>
        Error.Validation("Order.InvalidOrderLines", "Order must have at least one line.");

    public static Error SplitAmountMismatch(decimal total, decimal sumPayments) =>
        Error.Validation("Order.SplitAmountMismatch",
            $"Split payment total ({sumPayments}) must equal order total ({total}).");
}
