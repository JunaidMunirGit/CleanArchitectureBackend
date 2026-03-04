using SharedKernel;

namespace Domain.Products;

public sealed record ProductCreatedDomainEvent(Guid ProductId, string Sku) : IDomainEvent;
