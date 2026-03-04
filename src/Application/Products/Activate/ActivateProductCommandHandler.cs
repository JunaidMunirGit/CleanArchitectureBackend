using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Activate;

internal sealed class ActivateProductCommandHandler(IApplicationDbContext context)
    : ICommandHandler<ActivateProductCommand>
{
    public async Task<Result> Handle(ActivateProductCommand command, CancellationToken cancellationToken)
    {
        Domain.Products.Product? product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id && !p.IsDeleted, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.Id));
        }

        product.IsActive = true;
        product.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
