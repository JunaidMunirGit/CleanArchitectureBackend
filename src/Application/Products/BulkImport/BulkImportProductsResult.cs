namespace Application.Products.BulkImport;

public sealed class BulkImportProductsResult
{
    public int Total { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkImportItemResult> Results { get; set; } = [];
}

public sealed class BulkImportItemResult
{
    public int Index { get; set; }
    public string Sku { get; set; } = string.Empty;
    public bool Success { get; set; }
    public Guid? ProductId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
