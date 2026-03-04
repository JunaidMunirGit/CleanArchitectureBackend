using Application.Abstractions.Messaging;
using Application.Products.Delete;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class DeleteProduct : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/v1/products/{id:guid}", async (
            Guid id,
            [Microsoft.AspNetCore.Mvc.FromBody] DeleteProductRequest request,
            ICommandHandler<DeleteProductCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteProductCommand
            {
                Id = id,
                RowVersion = request.RowVersionBase64 is not null ? Convert.FromBase64String(request.RowVersionBase64) : []
            };
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission(ProductPermissions.ProductsWrite)
        .WithName("DeleteProduct")
        .Produces(204)
        .ProducesProblem(404)
        .ProducesProblem(409);
    }

    public sealed class DeleteProductRequest
    {
        public string? RowVersionBase64 { get; set; }
    }
}
