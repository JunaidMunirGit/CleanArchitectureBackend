using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetPrices;

internal sealed class GetProductPricesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetProductPricesQuery, List<ProductPriceResponse>>
{
    public async Task<Result<List<ProductPriceResponse>>> Handle(GetProductPricesQuery query, CancellationToken cancellationToken)
    {
        bool productExists = await context.Products.AnyAsync(p => p.Id == query.ProductId && !p.IsDeleted, cancellationToken);
        if (!productExists)
        {
            return Result.Failure<List<ProductPriceResponse>>(ProductErrors.NotFound(query.ProductId));
        }

        IQueryable<ProductPrice> pricesQuery = context.ProductPrices
            .AsNoTracking()
            .Where(p => p.ProductId == query.ProductId);

        if (query.BranchId.HasValue)
        {
            pricesQuery = pricesQuery.Where(p => p.BranchId == query.BranchId);
        }

        List<ProductPriceResponse> list = await pricesQuery
            .OrderBy(p => p.BranchId == null ? 0 : 1)
            .ThenBy(p => p.EffectiveFrom)
            .Select(p => new ProductPriceResponse
            {
                BranchId = p.BranchId,
                BranchName = p.Branch != null ? p.Branch.Name : null,
                Price = p.Price,
                EffectiveFrom = p.EffectiveFrom,
                EffectiveTo = p.EffectiveTo
            })
            .ToListAsync(cancellationToken);

        return list;
    }
}
