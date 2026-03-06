using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetSalesReport;

internal sealed class GetSalesReportQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSalesReportQuery, SalesReportResponse>
{
    public async Task<Result<SalesReportResponse>> Handle(GetSalesReportQuery query, CancellationToken cancellationToken)
    {
        DateTime from = query.From ?? DateTime.UtcNow.Date;
        DateTime to = (query.To ?? DateTime.UtcNow.Date).Date.AddDays(1);

        IQueryable<Domain.Orders.Order> ordersQuery = context.Orders
            .AsNoTracking()
            .Where(o => o.Status == Domain.Orders.OrderStatus.Completed && o.CreatedAt >= from && o.CreatedAt < to);

        decimal dailyRevenue = await ordersQuery.SumAsync(o => o.Total, cancellationToken);
        int totalOrders = await ordersQuery.CountAsync(cancellationToken);

        List<TopItemDto> topItems = await context.OrderLines
            .AsNoTracking()
            .Where(l => l.Order.CreatedAt >= from && l.Order.CreatedAt < to && l.Order.Status == Domain.Orders.OrderStatus.Completed)
            .GroupBy(l => new { l.ProductId, l.ProductName })
            .Select(g => new TopItemDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(l => l.Quantity),
                Revenue = g.Sum(l => l.LineTotal)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(20)
            .ToListAsync(cancellationToken);

        return new SalesReportResponse
        {
            DailyRevenue = dailyRevenue,
            TopItems = topItems,
            TotalOrders = totalOrders
        };
    }
}
