using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetByBarcode;

internal sealed class GetProductByBarcodeQueryHandler(
    IApplicationDbContext context,
    IProductBarcodeCache cache)
    : IQueryHandler<GetProductByBarcodeQuery, ProductByBarcodeResponse?>
{
    public async Task<Result<ProductByBarcodeResponse?>> Handle(GetProductByBarcodeQuery query, CancellationToken cancellationToken)
    {
        Guid? productId = await cache.GetProductIdByBarcodeAsync(query.Barcode, cancellationToken);
        if (productId is null)
        {
            var barcodeRow = await context.ProductBarcodes
                .AsNoTracking()
                .Where(b => b.Barcode == query.Barcode && !b.IsDeleted)
                .Select(b => new { b.ProductId })
                .FirstOrDefaultAsync(cancellationToken);

            if (barcodeRow is null)
            {
                return Result.Failure<ProductByBarcodeResponse?>(ProductErrors.NotFoundByBarcode(query.Barcode));
            }

            productId = barcodeRow.ProductId;
            await cache.SetAsync(query.Barcode, productId.Value, cancellationToken);
        }

        ProductByBarcodeResponse? product = await context.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && !p.IsDeleted)
            .Select(p => new ProductByBarcodeResponse
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                SellingPrice = p.SellingPrice,
                EffectivePrice = query.BranchId.HasValue
                    ? p.Prices.Where(pr => pr.BranchId == query.BranchId && (pr.EffectiveTo == null || pr.EffectiveTo > DateTime.UtcNow))
                        .OrderByDescending(pr => pr.EffectiveFrom)
                        .Select(pr => (decimal?)pr.Price)
                        .FirstOrDefault()
                    : null,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            await cache.InvalidateAsync(query.Barcode, cancellationToken);
            return Result.Failure<ProductByBarcodeResponse?>(ProductErrors.NotFoundByBarcode(query.Barcode));
        }

        return product;
    }
}
