using Application.Abstractions.Messaging;
using Domain.Products;

namespace Application.Products.Delete;

public sealed class DeleteProductCommand : ICommand
{
    public Guid Id { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
