using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetLowStock;

internal sealed class GetLowStockQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetLowStockQuery, List<LowStockItemDto>>
{
    public async Task<Result<List<LowStockItemDto>>> Handle(GetLowStockQuery query, CancellationToken cancellationToken)
    {
        List<LowStockItemDto> lowStock = await context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive && p.TrackInventory)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Sku,
                p.ReorderLevel,
                QuantityOnHand = context.ProductInventories
                    .Where(i => i.ProductId == p.Id)
                    .Select(i => i.QuantityOnHand)
                    .FirstOrDefault()
            })
            .Where(x => x.QuantityOnHand <= (int)x.ReorderLevel)
            .Select(x => new LowStockItemDto
            {
                ProductId = x.Id,
                ProductName = x.Name,
                Sku = x.Sku,
                QuantityOnHand = x.QuantityOnHand,
                ReorderLevel = x.ReorderLevel
            })
            .OrderBy(x => x.QuantityOnHand)
            .ToListAsync(cancellationToken);

        return lowStock;
    }
}
