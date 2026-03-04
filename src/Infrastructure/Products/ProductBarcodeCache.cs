using Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Products;

internal sealed class ProductBarcodeCache(IMemoryCache cache) : IProductBarcodeCache
{
    private const string KeyPrefix = "product_barcode:";
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(15);

    public Task<Guid?> GetProductIdByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        string key = KeyPrefix + barcode.Trim().ToUpperInvariant();
        bool found = cache.TryGetValue(key, out Guid? productId);
        return Task.FromResult(found ? productId : null);
    }

    public Task SetAsync(string barcode, Guid productId, CancellationToken cancellationToken = default)
    {
        string key = KeyPrefix + barcode.Trim().ToUpperInvariant();
        cache.Set(key, productId, DefaultTtl);
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(string barcode, CancellationToken cancellationToken = default)
    {
        string key = KeyPrefix + barcode.Trim().ToUpperInvariant();
        cache.Remove(key);
        return Task.CompletedTask;
    }
}
