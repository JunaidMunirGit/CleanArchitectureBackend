using Application.Abstractions.Messaging;
using Application.Products.Activate;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class ActivateProduct : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/v1/products/{id:guid}/activate", async (
            Guid id,
            ICommandHandler<ActivateProductCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new ActivateProductCommand { Id = id }, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission(ProductPermissions.ProductsWrite)
        .WithName("ActivateProduct")
        .Produces(204)
        .ProducesProblem(404);
    }
}
