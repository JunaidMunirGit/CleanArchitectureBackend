using Application.Abstractions.Messaging;
using Application.Orders.GetLowStock;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Pos;

internal sealed class GetLowStockEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/pos/inventory/low-stock", async (
            IQueryHandler<GetLowStockQuery, List<LowStockItemDto>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<LowStockItemDto>> result = await handler.Handle(new GetLowStockQuery(), cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(PosTags.Pos)
        .HasPermission(Permissions.ReportsRead)
        .WithName("GetLowStock")
        .Produces<List<LowStockItemDto>>(200);
    }
}
