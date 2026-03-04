using Application.Abstractions.Messaging;

namespace Application.Products.Activate;

public sealed class ActivateProductCommand : ICommand
{
    public Guid Id { get; set; }
}
