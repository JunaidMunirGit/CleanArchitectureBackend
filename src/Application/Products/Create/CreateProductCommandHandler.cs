using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Create;

internal sealed class CreateProductCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        bool skuExists = await context.Products
            .AnyAsync(p => p.Sku == command.Sku && !p.IsDeleted, cancellationToken);
        if (skuExists)
        {
            return Result.Failure<Guid>(ProductErrors.SkuExists(command.Sku));
        }

        foreach (string barcode in command.Barcodes)
        {
            bool barcodeExists = await context.ProductBarcodes
                .AnyAsync(b => b.Barcode == barcode && !b.IsDeleted, cancellationToken);
            if (barcodeExists)
            {
                return Result.Failure<Guid>(ProductErrors.BarcodeExists(barcode));
            }
        }

        bool categoryExists = await context.Categories
            .AnyAsync(c => c.Id == command.CategoryId && !c.IsDeleted, cancellationToken);
        if (!categoryExists)
        {
            return Result.Failure<Guid>(ProductErrors.CategoryNotFound(command.CategoryId));
        }

        bool unitExists = await context.UnitOfMeasures
            .AnyAsync(u => u.Id == command.UnitOfMeasureId, cancellationToken);
        if (!unitExists)
        {
            return Result.Failure<Guid>(ProductErrors.UnitOfMeasureNotFound(command.UnitOfMeasureId));
        }

        bool taxCategoryExists = await context.TaxCategories
            .AnyAsync(t => t.Id == command.TaxCategoryId, cancellationToken);
        if (!taxCategoryExists)
        {
            return Result.Failure<Guid>(ProductErrors.TaxCategoryNotFound(command.TaxCategoryId));
        }

        Guid? createdBy = userContext.UserId;
        DateTime utcNow = dateTimeProvider.UtcNow;

        var product = new Product
        {
            Sku = command.Sku,
            Name = command.Name,
            Description = command.Description,
            CategoryId = command.CategoryId,
            BrandId = command.BrandId,
            UnitOfMeasureId = command.UnitOfMeasureId,
            CostPrice = command.CostPrice,
            SellingPrice = command.SellingPrice,
            Discountable = command.Discountable,
            TrackInventory = command.TrackInventory,
            ReorderLevel = command.ReorderLevel,
            TaxCategoryId = command.TaxCategoryId,
            IsActive = true,
            ImageUrl = command.ImageUrl,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
            RowVersion = [1, 0, 0, 0, 0, 0, 0, 0]
        };

        product.Raise(new ProductCreatedDomainEvent(product.Id, product.Sku));

        string? primary = command.PrimaryBarcode ?? command.Barcodes.FirstOrDefault();
        foreach (string barcode in command.Barcodes)
        {
            product.Barcodes.Add(new ProductBarcode
            {
                Barcode = barcode,
                IsPrimary = barcode == primary
            });
        }

        context.Products.Add(product);

        await context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
