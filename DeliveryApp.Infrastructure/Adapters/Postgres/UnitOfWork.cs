using Newtonsoft.Json;
using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Postgres;

public sealed class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private bool _disposed;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task<bool> SaveEntities(CancellationToken cancellationToken = default)
    {
        // Перекладываем Domain Event в Outbox
        // После выполнения этого метода в DbContext будут находится и сам Aggregate и OutboxMessages
        await SaveDomainEventsInOutboxMessagesAsync();
        
        // Сохраняем весь DbContext одной транзакцией
        var savedEntriesCount = await _dbContext.SaveChangesAsync(cancellationToken);
        return savedEntriesCount > 0;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing) _dbContext.Dispose();
        _disposed = true;
    }
    
    private async Task SaveDomainEventsInOutboxMessagesAsync()
    {
        var outboxMessages = _dbContext.ChangeTracker
            .Entries<Aggregate>() // Получили агрегаты в которых есть доменные события
            .Select(x => x.Entity)
            .SelectMany(aggregate =>
                {
                    // Переложили в отдельную переменную
                    var domainEvents = aggregate.GetDomainEvents(); 
                    
                    // Очистили Domain Event в самих агрегатах (поскольку далее они будут отправлены и больше не нужны)
                    aggregate.ClearDomainEvents();
                    return domainEvents;
                }
            )
            .Select(domainEvent => new OutboxMessage
            {
                // Создали объект OutboxMessage на основе Domain Event
                Id = domainEvent.EventId,
                OccuredAtUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Content = JsonConvert.SerializeObject(
                    domainEvent,
                    new JsonSerializerSettings
                    {
                        // Эта настройка нужна, чтобы сериализовать Domain Event с указанием типов
                        // Если ее не указать, то десеарилизатор не поймет в какой тип восстанавливать сообщение
                        TypeNameHandling = TypeNameHandling.All
                    })
            })
            .ToList();

        // Добавляем OutboxMessages в dbContext
        // После выполнения этой строки в DbContext будут находится сам Aggregate и OutboxMessages
        await _dbContext.Set<OutboxMessage>().AddRangeAsync(outboxMessages);
    }

}