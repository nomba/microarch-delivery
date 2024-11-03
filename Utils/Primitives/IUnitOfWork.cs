namespace Primitives;

public interface IUnitOfWork
{
    Task<bool> SaveEntities(CancellationToken cancellationToken = default);
}