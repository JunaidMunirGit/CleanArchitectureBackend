using Application.Abstractions.Data;
using Domain.Orders;
using Domain.Products;
using Domain.Todos;
using Domain.Users;
using Infrastructure.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SharedKernel;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<TodoItem> TodoItems { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Brand> Brands { get; set; }

    public DbSet<UnitOfMeasure> UnitOfMeasures { get; set; }

    public DbSet<TaxCategory> TaxCategories { get; set; }

    public DbSet<Branch> Branches { get; set; }

    public DbSet<ProductBarcode> ProductBarcodes { get; set; }

    public DbSet<ProductPrice> ProductPrices { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderLine> OrderLines { get; set; }

    public DbSet<OrderPayment> OrderPayments { get; set; }

    public DbSet<ProductInventory> ProductInventories { get; set; }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction efTransaction = await Database.BeginTransactionAsync(cancellationToken);
        return new EfTransactionAdapter(efTransaction);
    }

    public void SetProductRowVersionOriginal(Product product, byte[] rowVersion)
    {
        Entry(product).Property(p => p.RowVersion).OriginalValue = rowVersion;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // When should you publish domain events?
        //
        // 1. BEFORE calling SaveChangesAsync
        //     - domain events are part of the same transaction
        //     - immediate consistency
        // 2. AFTER calling SaveChangesAsync
        //     - domain events are a separate transaction
        //     - eventual consistency
        //     - handlers can fail

        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync();

        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();

        await domainEventsDispatcher.DispatchAsync(domainEvents);
    }

    private sealed class EfTransactionAdapter(IDbContextTransaction transaction) : ITransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default) =>
            transaction.RollbackAsync(cancellationToken);

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
