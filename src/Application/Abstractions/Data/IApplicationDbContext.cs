using Domain.Products;
using Domain.Todos;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<TodoItem> TodoItems { get; }
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<UnitOfMeasure> UnitOfMeasures { get; }
    DbSet<TaxCategory> TaxCategories { get; }
    DbSet<Branch> Branches { get; }
    DbSet<ProductBarcode> ProductBarcodes { get; }
    DbSet<ProductPrice> ProductPrices { get; }
    DbSet<Domain.Orders.Order> Orders { get; }
    DbSet<Domain.Orders.OrderLine> OrderLines { get; }
    DbSet<Domain.Orders.OrderPayment> OrderPayments { get; }
    DbSet<Domain.Orders.ProductInventory> ProductInventories { get; }

    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    void SetProductRowVersionOriginal(Product product, byte[] rowVersion);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
