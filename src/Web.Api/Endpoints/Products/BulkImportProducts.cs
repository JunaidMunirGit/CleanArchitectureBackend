using Application.Abstractions.Messaging;
using Application.Products.BulkImport;
using Microsoft.Extensions.Primitives;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class BulkImportProducts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/products/bulk", async (
            HttpContext httpContext,
            BulkImportProductsCommand command,
            ICommandHandler<BulkImportProductsCommand, BulkImportProductsResult> handler,
            CancellationToken cancellationToken) =>
        {
            string? idempotencyKey = httpContext.Request.Headers.TryGetValue("Idempotency-Key", out StringValues headerValue) ? headerValue.ToString() : null;
            if (!string.IsNullOrWhiteSpace(idempotencyKey) && command.Items.Count > 0)
            {
                command.Items[0].IdempotencyKey = idempotencyKey;
            }

            Result<BulkImportProductsResult> result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission(ProductPermissions.ProductsBulk)
        .WithName("BulkImportProducts")
        .Produces<BulkImportProductsResult>(200)
        .ProducesProblem(400);
    }
}
