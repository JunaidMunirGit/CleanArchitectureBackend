using Application.Abstractions.Messaging;
using Application.Products.GetPrices;
using Application.Products.SetBranchPrice;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class ProductPrices : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/v1/products/{id:guid}/prices/branches/{branchId:guid}", async (
            Guid id,
            Guid branchId,
            SetBranchPriceRequest request,
            ICommandHandler<SetBranchPriceCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new SetBranchPriceCommand
            {
                ProductId = id,
                BranchId = branchId,
                Price = request.Price
            };
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission(ProductPermissions.ProductsWrite)
        .WithName("SetBranchPrice")
        .Produces(204)
        .ProducesProblem(404);

        app.MapGet("api/v1/products/{id:guid}/prices", async (
            Guid id,
            Guid? branchId,
            IQueryHandler<GetProductPricesQuery, List<ProductPriceResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<ProductPriceResponse>> result =
                await handler.Handle(new GetProductPricesQuery(id, branchId), cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission(ProductPermissions.ProductsRead)
        .WithName("GetProductPrices")
        .Produces<List<ProductPriceResponse>>(200)
        .ProducesProblem(404);
    }

    public sealed class SetBranchPriceRequest
    {
        public decimal Price { get; set; }
    }
}
