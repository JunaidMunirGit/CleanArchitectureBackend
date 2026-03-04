using Application.Abstractions.Messaging;
using Application.Products.GetById;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class GetProductById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/products/{id:guid}", async (
            Guid id,
            Guid? branchId,
            IQueryHandler<GetProductByIdQuery, ProductResponse?> handler,
            CancellationToken cancellationToken) =>
        {
            Result<ProductResponse?> result =
                await handler.Handle(new GetProductByIdQuery(id, branchId), cancellationToken);
            return result.Match(
                response => response is null ? Results.NotFound() : Results.Ok(response),
                CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission(ProductPermissions.ProductsRead)
        .WithName("GetProductById")
        .Produces<ProductResponse>(200)
        .ProducesProblem(404);
    }
}
