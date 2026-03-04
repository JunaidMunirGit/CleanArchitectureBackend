using SharedKernel;

namespace Domain.Products;

public sealed record ProductUpdatedDomainEvent(Guid ProductId, string Sku) : IDomainEvent;
