namespace Application.Abstractions.Caching;

/// <summary>
/// Cache for barcode -> product id lookups. Implement with IMemoryCache or Redis.
/// </summary>
public interface IProductBarcodeCache
{
    Task<Guid?> GetProductIdByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

    Task SetAsync(string barcode, Guid productId, CancellationToken cancellationToken = default);

    Task InvalidateAsync(string barcode, CancellationToken cancellationToken = default);
}
