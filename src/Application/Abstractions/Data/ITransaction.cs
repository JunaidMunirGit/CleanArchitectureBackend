namespace Application.Abstractions.Data;

/// <summary>
/// Represents a database transaction. Call <see cref="CommitAsync"/> to commit, or dispose to roll back.
/// </summary>
public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}
