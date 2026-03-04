using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.GetList;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class GetProductsList : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/products", async (
            string? q,
            string? sku,
            string? barcode,
            Guid? categoryId,
            bool? isActive,
            decimal? minPrice,
            decimal? maxPrice,
            Guid? branchId,
            int? pageNumber,
            int? pageSize,
            string? sortBy,
            string? sortDir,
            IQueryHandler<GetProductsQuery, PagedResult<ProductListItemResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductsQuery
            {
                Q = q,
                Sku = sku,
                Barcode = barcode,
                CategoryId = categoryId,
                IsActive = isActive,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                BranchId = branchId,
                PageNumber = pageNumber ?? 1,
                PageSize = Math.Clamp(pageSize ?? 20, 1, 100),
                SortBy = sortBy ?? "UpdatedAt",
                SortDir = sortDir ?? "desc"
            };
            Result<PagedResult<ProductListItemResponse>> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission(ProductPermissions.ProductsRead)
        .WithName("GetProducts")
        .Produces<PagedResult<ProductListItemResponse>>(200);
    }
}
