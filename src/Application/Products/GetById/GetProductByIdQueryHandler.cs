using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetById;

internal sealed class GetProductByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetProductByIdQuery, ProductResponse?>
{
    public async Task<Result<ProductResponse?>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        ProductResponse? product = await context.Products
            .AsNoTracking()
            .Where(p => p.Id == query.Id && !p.IsDeleted)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                UnitOfMeasureId = p.UnitOfMeasureId,
                UnitOfMeasureName = p.UnitOfMeasure.Name,
                CostPrice = p.CostPrice,
                SellingPrice = p.SellingPrice,
                EffectivePrice = query.BranchId.HasValue
                    ? p.Prices.Where(pr => pr.BranchId == query.BranchId && (pr.EffectiveTo == null || pr.EffectiveTo > DateTime.UtcNow))
                        .OrderByDescending(pr => pr.EffectiveFrom)
                        .Select(pr => pr.Price)
                        .FirstOrDefault()
                    : null,
                Discountable = p.Discountable,
                TrackInventory = p.TrackInventory,
                ReorderLevel = p.ReorderLevel,
                TaxCategoryId = p.TaxCategoryId,
                TaxCategoryName = p.TaxCategory.Name,
                IsActive = p.IsActive,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                RowVersion = p.RowVersion,
                Barcodes = p.Barcodes.Where(b => !b.IsDeleted).Select(b => b.Barcode).ToList(),
                PrimaryBarcode = p.Barcodes.Where(b => !b.IsDeleted && b.IsPrimary).Select(b => b.Barcode).FirstOrDefault()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductResponse?>(ProductErrors.NotFound(query.Id));
        }

        return product;
    }
}
