using Application.Abstractions.Messaging;
using Application.Orders.Create;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Pos;

internal sealed class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/pos/orders", async (
            CreateOrderCommand command,
            ICommandHandler<CreateOrderCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await handler.Handle(command, cancellationToken);
            return result.Match(
                id => Results.Created($"api/v1/pos/orders/{id}", new { id }),
                CustomResults.Problem);
        })
        .WithTags(PosTags.Pos)
        .HasPermission(Permissions.OrdersWrite)
        .WithName("CreateOrder")
        .Produces<object>(StatusCodes.Status201Created)
        .ProducesProblem(400)
        .ProducesProblem(404);
    }
}
