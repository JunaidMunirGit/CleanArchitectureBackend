using FluentValidation;

namespace Application.Orders.Create;

internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(c => c.Lines).NotEmpty().WithMessage("Order must have at least one line.");
        RuleFor(c => c.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(c => c.TaxPercent).InclusiveBetween(0, 100);
        RuleForEach(c => c.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.Quantity).GreaterThan(0);
        });
        When(c => c.PaymentMethod == Domain.Orders.PaymentMethod.Split, () =>
        {
            RuleFor(c => c.SplitPayments).NotEmpty().WithMessage("Split payment requires at least one payment.");
        });
    }
}
