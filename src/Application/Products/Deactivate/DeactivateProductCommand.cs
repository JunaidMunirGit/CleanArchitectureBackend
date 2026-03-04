using Application.Abstractions.Messaging;

namespace Application.Products.Deactivate;

public sealed class DeactivateProductCommand : ICommand
{
    public Guid Id { get; set; }
}
