using Application.Abstractions.Messaging;

namespace Application.Products.GetPrices;

public sealed record GetProductPricesQuery(Guid ProductId, Guid? BranchId = null) : IQuery<List<ProductPriceResponse>>;
