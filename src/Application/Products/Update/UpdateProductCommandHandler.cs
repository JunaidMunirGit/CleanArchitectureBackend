using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Update;

internal sealed class UpdateProductCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider,
    IProductBarcodeCache barcodeCache)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.Barcodes.Where(b => !b.IsDeleted))
            .FirstOrDefaultAsync(p => p.Id == command.Id && !p.IsDeleted, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.Id));
        }

        context.SetProductRowVersionOriginal(product, command.RowVersion);

        foreach (string barcode in command.Barcodes)
        {
            bool existsElsewhere = await context.ProductBarcodes
                .AnyAsync(b => b.Barcode == barcode && !b.IsDeleted && b.ProductId != command.Id, cancellationToken);
            if (existsElsewhere)
            {
                return Result.Failure(ProductErrors.BarcodeExists(barcode));
            }
        }

        bool categoryExists = await context.Categories.AnyAsync(c => c.Id == command.CategoryId && !c.IsDeleted, cancellationToken);
        if (!categoryExists)
        {
            return Result.Failure(ProductErrors.CategoryNotFound(command.CategoryId));
        }

        Guid? updatedBy = userContext.UserId;
        DateTime utcNow = dateTimeProvider.UtcNow;

        product.Name = command.Name;
        product.Description = command.Description;
        product.CategoryId = command.CategoryId;
        product.BrandId = command.BrandId;
        product.UnitOfMeasureId = command.UnitOfMeasureId;
        product.CostPrice = command.CostPrice;
        product.SellingPrice = command.SellingPrice;
        product.Discountable = command.Discountable;
        product.TrackInventory = command.TrackInventory;
        product.ReorderLevel = command.ReorderLevel;
        product.TaxCategoryId = command.TaxCategoryId;
        product.ImageUrl = command.ImageUrl;
        product.UpdatedAt = utcNow;
        product.UpdatedBy = updatedBy;

        var existingBarcodes = product.Barcodes.Select(b => b.Barcode).ToHashSet();
        var newBarcodes = command.Barcodes.ToHashSet();

        foreach (ProductBarcode existing in product.Barcodes.ToList())
        {
            if (!newBarcodes.Contains(existing.Barcode))
            {
                existing.IsDeleted = true;
                await barcodeCache.InvalidateAsync(existing.Barcode, cancellationToken);
            }
            else
            {
                existing.IsPrimary = existing.Barcode == (command.PrimaryBarcode ?? command.Barcodes.FirstOrDefault());
            }
        }

        foreach (string barcode in newBarcodes.Where(b => !existingBarcodes.Contains(b)))
        {
            product.Barcodes.Add(new ProductBarcode
            {
                Barcode = barcode,
                IsPrimary = barcode == (command.PrimaryBarcode ?? command.Barcodes.FirstOrDefault())
            });
        }

        product.Raise(new ProductUpdatedDomainEvent(product.Id, product.Sku));

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(ProductErrors.ConcurrencyConflict());
        }

        return Result.Success();
    }
}
