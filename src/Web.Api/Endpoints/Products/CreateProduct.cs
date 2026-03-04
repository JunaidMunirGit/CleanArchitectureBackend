using Application.Abstractions.Messaging;
using Application.Products.Create;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class CreateProduct : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/products", async (
            CreateProductCommand command,
            ICommandHandler<CreateProductCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await handler.Handle(command, cancellationToken);
            return result.Match(id => Results.Created($"api/v1/products/{id}", new { id }), CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission(ProductPermissions.ProductsWrite)
        .WithName("CreateProduct")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(400)
        .ProducesProblem(409);
    }
}
