using Application.Abstractions.Messaging;

namespace Application.Orders.GetSalesReport;

public sealed class GetSalesReportQuery : IQuery<SalesReportResponse>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public sealed class SalesReportResponse
{
    public decimal DailyRevenue { get; set; }
    public List<TopItemDto> TopItems { get; set; } = [];
    public int TotalOrders { get; set; }
}

public sealed class TopItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
