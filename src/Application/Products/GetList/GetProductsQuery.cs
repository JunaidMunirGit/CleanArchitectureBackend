using Application.Abstractions.Data;
using Application.Abstractions.Messaging;

namespace Application.Products.GetList;

public sealed class GetProductsQuery : IQuery<PagedResult<ProductListItemResponse>>
{
    public string? Q { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? BranchId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "UpdatedAt";
    public string SortDir { get; set; } = "desc";
}
