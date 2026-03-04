using Application.Abstractions.Messaging;
using Application.Products.GetByBarcode;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class GetProductByBarcode : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/products/by-barcode/{barcode}", async (
            string barcode,
            Guid? branchId,
            IQueryHandler<GetProductByBarcodeQuery, ProductByBarcodeResponse?> handler,
            CancellationToken cancellationToken) =>
        {
            Result<ProductByBarcodeResponse?> result =
                await handler.Handle(new GetProductByBarcodeQuery(barcode, branchId), cancellationToken);
            return result.Match(
                response => response is null ? Results.NotFound() : Results.Ok(response),
                CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission(ProductPermissions.ProductsRead)
        .WithName("GetProductByBarcode")
        .Produces<ProductByBarcodeResponse>(200)
        .ProducesProblem(404);
    }
}
