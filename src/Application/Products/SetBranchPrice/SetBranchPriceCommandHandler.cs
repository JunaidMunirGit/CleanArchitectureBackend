using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SetBranchPrice;

internal sealed class SetBranchPriceCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<SetBranchPriceCommand>
{
    public async Task<Result> Handle(SetBranchPriceCommand command, CancellationToken cancellationToken)
    {
        bool productExists = await context.Products.AnyAsync(p => p.Id == command.ProductId && !p.IsDeleted, cancellationToken);
        if (!productExists)
        {
            return Result.Failure(ProductErrors.NotFound(command.ProductId));
        }

        bool branchExists = await context.Branches.AnyAsync(b => b.Id == command.BranchId && !b.IsDeleted, cancellationToken);
        if (!branchExists)
        {
            return Result.Failure(ProductErrors.BranchNotFound(command.BranchId));
        }

        DateTime utcNow = dateTimeProvider.UtcNow;
        Guid? userId = userContext.UserId;

        ProductPrice? existing = await context.ProductPrices
            .FirstOrDefaultAsync(p =>
                p.ProductId == command.ProductId &&
                p.BranchId == command.BranchId &&
                (p.EffectiveTo == null || p.EffectiveTo > utcNow), cancellationToken);

        if (existing is { } ex)
        {
            ex.EffectiveTo = utcNow;
        }

        context.ProductPrices.Add(new ProductPrice
        {
            ProductId = command.ProductId,
            BranchId = command.BranchId,
            Price = command.Price,
            EffectiveFrom = utcNow,
            UpdatedAt = utcNow,
            UpdatedBy = userId
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
