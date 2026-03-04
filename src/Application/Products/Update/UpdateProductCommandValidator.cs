using FluentValidation;

namespace Application.Products.Update;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
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
