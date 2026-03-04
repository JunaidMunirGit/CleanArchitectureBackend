using Application.Abstractions.Messaging;

namespace Application.Products.SetBranchPrice;

public sealed class SetBranchPriceCommand : ICommand
{
    public Guid ProductId { get; set; }
    public Guid BranchId { get; set; }
    public decimal Price { get; set; }
}
