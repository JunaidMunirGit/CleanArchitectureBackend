using FluentValidation;

namespace Application.Products.Create;

internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(c => c.Sku).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Description).MaximumLength(2000).When(c => c.Description is not null);
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.UnitOfMeasureId).NotEmpty();
        RuleFor(c => c.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(c => c.ReorderLevel).GreaterThanOrEqualTo(0);
        RuleFor(c => c.TaxCategoryId).NotEmpty();
        RuleForEach(c => c.Barcodes).NotEmpty().MaximumLength(100);
    }
}
