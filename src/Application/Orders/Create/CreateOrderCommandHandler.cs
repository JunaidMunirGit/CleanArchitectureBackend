using System.Globalization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Authentication;
using Domain.Orders;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Create;

internal sealed class CreateOrderCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateOrderCommand, Guid>
{
    private const string OrderNumberPrefix = "ORD";
    private const int OrderNumberSequenceLength = 6;

    public async Task<Result<Guid>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.Lines.Count == 0)
        {
            return Result.Failure<Guid>(OrderErrors.InvalidOrderLines);
        }

        Guid? createdBy = userContext.UserId;
        DateTime utcNow = dateTimeProvider.UtcNow;

        // Load products and validate
        var productIds = command.Lines.Select(l => l.ProductId).Distinct().ToList();
        Dictionary<Guid, Domain.Products.Product> products = await context.Products
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted && p.IsActive)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        foreach (CreateOrderLineRequest line in command.Lines)
        {
            if (!products.TryGetValue(line.ProductId, out Domain.Products.Product? product))
            {
                return Result.Failure<Guid>(OrderErrors.ProductNotFound(line.ProductId));
            }

            if (line.Quantity <= 0)
            {
                return Result.Failure<Guid>(Error.Validation("Order.InvalidQuantity", "Quantity must be positive."));
            }

            if (product.TrackInventory)
            {
                Domain.Orders.ProductInventory? inv = await context.ProductInventories.FindAsync([line.ProductId], cancellationToken);
                int onHand = inv?.QuantityOnHand ?? 0;
                if (onHand < line.Quantity)
                {
                    return Result.Failure<Guid>(OrderErrors.InsufficientStock(line.ProductId, product.Name, line.Quantity, onHand));
                }
            }
        }

        string orderNumber = await GenerateOrderNumberAsync(utcNow, cancellationToken);

        decimal subTotal = 0;
        var lines = new List<OrderLine>();
        foreach (CreateOrderLineRequest lineReq in command.Lines)
        {
            Domain.Products.Product product = products[lineReq.ProductId];
            decimal unitPrice = product.SellingPrice;
            decimal lineTotal = unitPrice * lineReq.Quantity;
            subTotal += lineTotal;
            lines.Add(new OrderLine
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = lineReq.Quantity,
                UnitPrice = unitPrice,
                LineTotal = lineTotal
            });
        }

        decimal discountAmount = command.DiscountAmount;
        decimal afterDiscount = subTotal - discountAmount;
        decimal taxAmount = Math.Round(afterDiscount * (command.TaxPercent / 100m), 2, MidpointRounding.AwayFromZero);
        decimal total = afterDiscount + taxAmount;

        if (command.PaymentMethod == PaymentMethod.Split)
        {
            if (command.SplitPayments == null || command.SplitPayments.Count == 0)
            {
                return Result.Failure<Guid>(Error.Validation("Order.SplitRequired", "Split payment requires at least one payment entry."));
            }

            decimal sumPayments = command.SplitPayments.Sum(p => p.Amount);
            if (Math.Abs(sumPayments - total) > 0.01m)
            {
                return Result.Failure<Guid>(OrderErrors.SplitAmountMismatch(total, sumPayments));
            }
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Status = OrderStatus.Completed,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TaxAmount = taxAmount,
            Total = total,
            PaymentMethod = command.PaymentMethod,
            CreatedAt = utcNow,
            CreatedBy = createdBy,
            CompletedAt = utcNow
        };

        foreach (OrderLine line in lines)
        {
            line.OrderId = order.Id;
            context.OrderLines.Add(line);
        }

        if (command.PaymentMethod == PaymentMethod.Split && command.SplitPayments != null)
        {
            foreach (CreateOrderPaymentRequest pay in command.SplitPayments)
            {
                context.OrderPayments.Add(new OrderPayment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Method = pay.Method,
                    Amount = pay.Amount
                });
            }
        }
        else
        {
            context.OrderPayments.Add(new OrderPayment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Method = command.PaymentMethod,
                Amount = total
            });
        }

        context.Orders.Add(order);

        // Decrement inventory for tracked products (inventory row must exist and have been validated above)
        foreach (CreateOrderLineRequest lineReq in command.Lines)
        {
            Domain.Products.Product product = products[lineReq.ProductId];
            if (!product.TrackInventory)
            {
                continue;
            }

            Domain.Orders.ProductInventory? inv = await context.ProductInventories.FindAsync([lineReq.ProductId], cancellationToken);
            if (inv != null)
            {
                inv.QuantityOnHand -= lineReq.Quantity;
                inv.UpdatedAt = utcNow;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return order.Id;
    }

    private async Task<string> GenerateOrderNumberAsync(DateTime date, CancellationToken cancellationToken)
    {
        string datePart = date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        int seq = await context.Orders
            .Where(o => o.OrderNumber.StartsWith(OrderNumberPrefix + "-" + datePart))
            .CountAsync(cancellationToken) + 1;
        return $"{OrderNumberPrefix}-{datePart}-{seq.ToString(CultureInfo.InvariantCulture).PadLeft(OrderNumberSequenceLength, '0')}";
    }
}
