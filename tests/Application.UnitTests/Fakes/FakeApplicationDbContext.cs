using Application.Abstractions.Data;
using Domain.Orders;
using Domain.Products;
using Domain.Todos;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Fakes;

/// <summary>
/// In-memory implementation of <see cref="IApplicationDbContext"/> for unit tests.
/// Uses a unique database name per instance so tests are isolated.
/// </summary>
public sealed class FakeApplicationDbContext : DbContext, IApplicationDbContext
{
    public FakeApplicationDbContext(DbContextOptions<FakeApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
    public DbSet<TaxCategory> TaxCategories => Set<TaxCategory>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderPayment> OrderPayments => Set<OrderPayment>();
    public DbSet<ProductInventory> ProductInventories => Set<ProductInventory>();

    public Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<ITransaction>(new NoOpTransaction());

    public void SetProductRowVersionOriginal(Product product, byte[] rowVersion)
    {
        // In-memory fake: no-op for concurrency token; tests can still run.
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(e => e.Id);
        });
        modelBuilder.Entity<TodoItem>(b =>
        {
            b.HasKey(e => e.Id);
        });
        modelBuilder.Entity<Category>(b =>
        {
            b.HasKey(e => e.Id);
        });
        modelBuilder.Entity<Brand>(b =>
        {
            b.HasKey(e => e.Id);
        });
        modelBuilder.Entity<UnitOfMeasure>(b =>
        {
            b.HasKey(e => e.Id);
        });
        modelBuilder.Entity<TaxCategory>(b =>
        {
            b.HasKey(e => e.Id);
        });
        modelBuilder.Entity<Branch>(b =>
        {
            b.HasKey(e => e.Id);
        });
        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId);
            b.HasOne(e => e.Brand).WithMany().HasForeignKey(e => e.BrandId).IsRequired(false);
            b.HasOne(e => e.UnitOfMeasure).WithMany().HasForeignKey(e => e.UnitOfMeasureId);
            b.HasOne(e => e.TaxCategory).WithMany().HasForeignKey(e => e.TaxCategoryId);
            b.HasMany(e => e.Barcodes).WithOne(x => x.Product).HasForeignKey(x => x.ProductId);
            b.HasMany(e => e.Prices).WithOne(x => x.Product).HasForeignKey(x => x.ProductId);
        });
        modelBuilder.Entity<ProductBarcode>(b =>
        {
            b.HasKey(e => e.Id);
        });
        modelBuilder.Entity<ProductPrice>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId).IsRequired(false);
        });
        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasMany(e => e.Lines).WithOne(x => x.Order).HasForeignKey(x => x.OrderId);
            b.HasMany(e => e.Payments).WithOne(x => x.Order).HasForeignKey(x => x.OrderId);
        });
        modelBuilder.Entity<OrderLine>(b =>
        {
            b.HasKey(e => e.Id);
        });
        modelBuilder.Entity<OrderPayment>(b =>
        {
            b.HasKey(e => e.Id);
        });
        modelBuilder.Entity<ProductInventory>(b =>
        {
            b.HasKey(e => e.ProductId);
        });
    }

    /// <summary>
    /// Creates an in-memory context with a unique database name for test isolation.
    /// </summary>
    public static FakeApplicationDbContext Create()
    {
        DbContextOptions<FakeApplicationDbContext> options = new DbContextOptionsBuilder<FakeApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new FakeApplicationDbContext(options);
    }

    private sealed class NoOpTransaction : ITransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
