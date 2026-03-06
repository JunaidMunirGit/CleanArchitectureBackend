using Application.Abstractions.Messaging;
using Application.Orders.GetSalesReport;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Pos;

internal sealed class GetSalesReportEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/pos/sales/report", async (
            DateTime? from,
            DateTime? to,
            IQueryHandler<GetSalesReportQuery, SalesReportResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSalesReportQuery { From = from, To = to };
            Result<SalesReportResponse> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(PosTags.Pos)
        .HasPermission(Permissions.ReportsRead)
        .WithName("GetSalesReport")
        .Produces<SalesReportResponse>(200)
        .ProducesProblem(400);
    }
}
