using CSharpFunctionalExtensions;

namespace Primitives;

public abstract class Aggregate : Entity<Guid>, IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = new();

    protected Aggregate(Guid id) : base(id)
    {
    }

    protected Aggregate()
    {
    }

    public IReadOnlyCollection<DomainEvent> GetDomainEvents()
    {
        return _domainEvents.ToList();
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}

public interface IAggregateRoot;