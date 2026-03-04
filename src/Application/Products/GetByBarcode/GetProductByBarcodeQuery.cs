using Application.Abstractions.Messaging;

namespace Application.Products.GetByBarcode;

public sealed record GetProductByBarcodeQuery(string Barcode, Guid? BranchId = null) : IQuery<ProductByBarcodeResponse?>;
