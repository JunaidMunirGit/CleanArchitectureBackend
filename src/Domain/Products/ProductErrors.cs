using SharedKernel;

namespace Domain.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Product.NotFound", $"Product with id '{id}' was not found.");

    public static Error NotFoundBySku(string sku) =>
        Error.NotFound("Product.NotFoundBySku", $"Product with SKU '{sku}' was not found.");

    public static Error NotFoundByBarcode(string barcode) =>
        Error.NotFound("Product.NotFoundByBarcode", $"Product with barcode '{barcode}' was not found.");

    public static Error SkuExists(string sku) =>
        Error.Conflict("Product.SkuExists", $"Product with SKU '{sku}' already exists.");

    public static Error BarcodeExists(string barcode) =>
        Error.Conflict("Product.BarcodeExists", $"Barcode '{barcode}' is already assigned to another product.");

    public static Error ConcurrencyConflict() =>
        Error.Conflict("Product.ConcurrencyConflict", "The product was modified by another process. Please refresh and try again.");

    public static Error CategoryNotFound(Guid id) =>
        Error.NotFound("Product.CategoryNotFound", $"Category with id '{id}' was not found.");

    public static Error BranchNotFound(Guid id) =>
        Error.NotFound("Product.BranchNotFound", $"Branch with id '{id}' was not found.");
}
