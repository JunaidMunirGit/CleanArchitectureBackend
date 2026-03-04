using Application.Abstractions.Messaging;
using Application.Products.Deactivate;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class DeactivateProduct : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/v1/products/{id:guid}/deactivate", async (
            Guid id,
            ICommandHandler<DeactivateProductCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new DeactivateProductCommand { Id = id }, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission(ProductPermissions.ProductsWrite)
        .WithName("DeactivateProduct")
        .Produces(204)
        .ProducesProblem(404);
    }
}
