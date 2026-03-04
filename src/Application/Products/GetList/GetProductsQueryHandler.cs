using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetList;

internal sealed class GetProductsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductListItemResponse>>
{
    public async Task<Result<PagedResult<ProductListItemResponse>>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Products.Product> dbQuery = context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            string q = query.Q.Trim().ToUpperInvariant();
            // EF Core does not translate Contains(string, StringComparison); ToUpperInvariant is required for SQL translation.
#pragma warning disable CA1862
            dbQuery = dbQuery.Where(p =>
                p.Name.ToUpperInvariant().Contains(q) || p.Sku.ToUpperInvariant().Contains(q));
#pragma warning restore CA1862
        }

        if (!string.IsNullOrWhiteSpace(query.Sku))
        {
            dbQuery = dbQuery.Where(p => p.Sku == query.Sku);
        }

        if (!string.IsNullOrWhiteSpace(query.Barcode))
        {
            dbQuery = dbQuery.Where(p => p.Barcodes.Any(b => b.Barcode == query.Barcode && !b.IsDeleted));
        }

        if (query.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.CategoryId == query.CategoryId);
        }

        if (query.IsActive.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.IsActive == query.IsActive);
        }

        if (query.MinPrice.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.SellingPrice >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.SellingPrice <= query.MaxPrice.Value);
        }

        int totalCount = await dbQuery.CountAsync(cancellationToken);

        string sortBy = query.SortBy?.ToUpperInvariant() ?? "UPDATEDAT";
        bool desc = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        IOrderedQueryable<Domain.Products.Product>? ordered = sortBy switch
        {
            "NAME" => desc ? dbQuery.OrderByDescending(p => p.Name) : dbQuery.OrderBy(p => p.Name),
            "SKU" => desc ? dbQuery.OrderByDescending(p => p.Sku) : dbQuery.OrderBy(p => p.Sku),
            "SELLINGPRICE" => desc ? dbQuery.OrderByDescending(p => p.SellingPrice) : dbQuery.OrderBy(p => p.SellingPrice),
            "UPDATEDAT" => desc ? dbQuery.OrderByDescending(p => p.UpdatedAt) : dbQuery.OrderBy(p => p.UpdatedAt),
            _ => dbQuery.OrderByDescending(p => p.UpdatedAt)
        };

        List<ProductListItemResponse> items = await ordered
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductListItemResponse
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                SellingPrice = p.SellingPrice,
                EffectivePrice = query.BranchId.HasValue
                    ? p.Prices.Where(pr => pr.BranchId == query.BranchId && (pr.EffectiveTo == null || pr.EffectiveTo > DateTime.UtcNow))
                        .OrderByDescending(pr => pr.EffectiveFrom)
                        .Select(pr => (decimal?)pr.Price)
                        .FirstOrDefault()
                    : null,
                IsActive = p.IsActive,
                UpdatedAt = p.UpdatedAt,
                PrimaryBarcode = p.Barcodes.Where(b => b.IsPrimary).Select(b => b.Barcode).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ProductListItemResponse>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };

        return result;
    }
}
