# Product module – indexes, caching, performance

## Indexes (EF Core)

Configured in `Infrastructure/Products/*Configuration.cs`:

- **Product**
  - Unique: `SKU` (filtered `is_deleted = 0`)
  - Non-unique: `Name`, `CategoryId` + `IsDeleted`, `IsActive`
- **ProductBarcode**
  - Unique: `Barcode` (filtered `is_deleted = 0`)
  - Non-unique: `ProductId`
- **ProductPrice**
  - Composite: `(ProductId, BranchId)` for branch price lookups

Barcode lookup uses the unique index on `ProductBarcode.Barcode` and `AsNoTracking()` for read-only queries.

## Caching

- **Interface:** `Application.Abstractions.Caching.IProductBarcodeCache`
- **Implementation:** `Infrastructure.Products.ProductBarcodeCache` (IMemoryCache), TTL 15 minutes.
- **Usage:** `GetProductByBarcodeQueryHandler` checks cache first; on miss loads from DB, then caches `barcode → productId`. Invalidation on product update/delete when barcodes change.
- **Redis:** Replace the implementation with a Redis-backed cache (same interface) for multi-instance deployments.

## Concurrency

- **Product:** `RowVersion` (SQL Server `rowversion`) as concurrency token. Update/delete send `RowVersion`; on conflict handlers catch `DbUpdateConcurrencyException` and return 409 with `ProductErrors.ConcurrencyConflict()`.

## Soft delete

- **Global filter:** `HasQueryFilter(p => !p.IsDeleted)` on Product, Category, Brand, UnitOfMeasure, TaxCategory, Branch, ProductBarcode. All list/get exclude deleted rows; delete sets `IsDeleted = true` and updates `UpdatedAt`.

## Query patterns

- List/search: `AsNoTracking()`, filters, then `Skip`/`Take` after `OrderBy`.
- GetById / GetByBarcode: single `AsNoTracking()` projection to DTO.
- Branch effective price: subquery on `ProductPrices` with `BranchId` and `EffectiveTo` (null or > UtcNow).

## Optional: compiled query for barcode

For very high throughput, define a compiled query for barcode → product id and use it in `GetProductByBarcodeQueryHandler` instead of the current LINQ. Same index and cache apply.
