using Application.Abstractions.Messaging;
using Application.Products.Update;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class UpdateProduct : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/v1/products/{id:guid}", async (
            Guid id,
            UpdateProductCommand command,
            ICommandHandler<UpdateProductCommand> handler,
            CancellationToken cancellationToken) =>
        {
            command.Id = id;
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission(ProductPermissions.ProductsWrite)
        .WithName("UpdateProduct")
        .Produces(204)
        .ProducesProblem(404)
        .ProducesProblem(409);
    }
}
