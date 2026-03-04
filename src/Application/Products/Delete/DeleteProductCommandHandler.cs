using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Delete;

internal sealed class DeleteProductCommandHandler(IApplicationDbContext context, IProductBarcodeCache cache)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.Barcodes)
            .FirstOrDefaultAsync(p => p.Id == command.Id && !p.IsDeleted, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.Id));
        }

        context.SetProductRowVersionOriginal(product, command.RowVersion);
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;

        foreach (ProductBarcode b in product.Barcodes)
        {
            b.IsDeleted = true;
            await cache.InvalidateAsync(b.Barcode, cancellationToken);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(ProductErrors.ConcurrencyConflict());
        }

        return Result.Success();
    }
}
