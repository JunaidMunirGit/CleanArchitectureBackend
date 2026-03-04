using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.BulkImport;

internal sealed class BulkImportProductsCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<BulkImportProductsCommand, BulkImportProductsResult>
{
    public async Task<Result<BulkImportProductsResult>> Handle(BulkImportProductsCommand command, CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        DateTime utcNow = dateTimeProvider.UtcNow;
        var result = new BulkImportProductsResult { Total = command.Items.Count };
        var results = new List<BulkImportItemResult>();

        if (command.AllOrNothing)
        {
            await using ITransaction transaction = await context.BeginTransactionAsync(cancellationToken);
            try
            {
                for (int i = 0; i < command.Items.Count; i++)
                {
                    BulkImportProductItem item = command.Items[i];
                    BulkImportItemResult itemResult = await UpsertOneAsync(item, i, userId, utcNow, cancellationToken);
                    results.Add(itemResult);
                    if (!itemResult.Success)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        result.Results = results;
                        result.SuccessCount = results.Count(r => r.Success);
                        result.FailedCount = results.Count(r => !r.Success);
                        return result;
                    }
                }
                await context.SaveChangesAsync(cancellationToken);
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].Success && !results[i].ProductId.HasValue)
                    {
                        Product? p = await context.Products.AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Sku == command.Items[i].Sku && !x.IsDeleted, cancellationToken);
                        if (p is not null)
                        {
                            results[i].ProductId = p.Id;
                        }
                    }
                }
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        else
        {
            for (int i = 0; i < command.Items.Count; i++)
            {
                BulkImportProductItem item = command.Items[i];
                BulkImportItemResult itemResult = await UpsertOneAsync(item, i, userId, utcNow, cancellationToken);
                if (itemResult.Success)
                {
                    try
                    {
                        await context.SaveChangesAsync(cancellationToken);
                        if (!itemResult.ProductId.HasValue)
                        {
                            Product? p = await context.Products.AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Sku == item.Sku && !x.IsDeleted, cancellationToken);
                            if (p is not null)
                            {
                                itemResult.ProductId = p.Id;
                            }
                        }
                    }
                    catch (DbUpdateException ex)
                    {
                        itemResult.Success = false;
                        itemResult.ProductId = null;
                        itemResult.ErrorCode = "Product.SaveFailed";
                        itemResult.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
                    }
                }
                results.Add(itemResult);
            }
        }

        result.Results = results;
        result.SuccessCount = results.Count(r => r.Success);
        result.FailedCount = results.Count(r => !r.Success);
        return result;
    }

    private async Task<BulkImportItemResult> UpsertOneAsync(
        BulkImportProductItem item,
        int index,
        Guid? userId,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var itemResult = new BulkImportItemResult { Index = index, Sku = item.Sku };

        Product? existing = await context.Products
            .Include(p => p.Barcodes)
            .FirstOrDefaultAsync(p => p.Sku == item.Sku && !p.IsDeleted, cancellationToken);

        if (existing is not null)
        {
            existing.Name = item.Name;
            existing.Description = item.Description;
            existing.CategoryId = item.CategoryId;
            existing.BrandId = item.BrandId;
            existing.UnitOfMeasureId = item.UnitOfMeasureId;
            existing.CostPrice = item.CostPrice;
            existing.SellingPrice = item.SellingPrice;
            existing.Discountable = item.Discountable;
            existing.TrackInventory = item.TrackInventory;
            existing.ReorderLevel = item.ReorderLevel;
            existing.TaxCategoryId = item.TaxCategoryId;
            existing.ImageUrl = item.ImageUrl;
            existing.UpdatedAt = utcNow;
            existing.UpdatedBy = userId;
            itemResult.ProductId = existing.Id;
            itemResult.Success = true;
            return itemResult;
        }

        bool categoryExists = await context.Categories.AnyAsync(c => c.Id == item.CategoryId && !c.IsDeleted, cancellationToken);
        if (!categoryExists)
        {
            itemResult.Success = false;
            itemResult.ErrorCode = "Product.CategoryNotFound";
            itemResult.ErrorMessage = $"Category {item.CategoryId} not found.";
            return itemResult;
        }

        foreach (string barcode in item.Barcodes)
        {
            bool barcodeExists = await context.ProductBarcodes.AnyAsync(b => b.Barcode == barcode && !b.IsDeleted, cancellationToken);
            if (barcodeExists)
            {
                itemResult.Success = false;
                itemResult.ErrorCode = "Product.BarcodeExists";
                itemResult.ErrorMessage = $"Barcode '{barcode}' already exists.";
                return itemResult;
            }
        }

        var product = new Product
        {
            Sku = item.Sku,
            Name = item.Name,
            Description = item.Description,
            CategoryId = item.CategoryId,
            BrandId = item.BrandId,
            UnitOfMeasureId = item.UnitOfMeasureId,
            CostPrice = item.CostPrice,
            SellingPrice = item.SellingPrice,
            Discountable = item.Discountable,
            TrackInventory = item.TrackInventory,
            ReorderLevel = item.ReorderLevel,
            TaxCategoryId = item.TaxCategoryId,
            ImageUrl = item.ImageUrl,
            IsActive = true,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            CreatedBy = userId,
            UpdatedBy = userId,
            RowVersion = [1, 0, 0, 0, 0, 0, 0, 0]
        };

        string? primary = item.PrimaryBarcode ?? item.Barcodes.FirstOrDefault();
        foreach (string barcode in item.Barcodes)
        {
            product.Barcodes.Add(new ProductBarcode
            {
                Barcode = barcode,
                IsPrimary = barcode == primary
            });
        }

        context.Products.Add(product);
        itemResult.ProductId = product.Id;
        itemResult.Success = true;
        return itemResult;
    }
}
