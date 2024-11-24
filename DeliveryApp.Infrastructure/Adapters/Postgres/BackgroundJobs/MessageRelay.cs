using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Primitives;
using Quartz;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.BackgroundJobs;

[DisallowConcurrentExecution]
public class MessageRelayJob : IJob
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublisher _publisher;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MessageRelayJob(ApplicationDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // Получаем все DomainEvents, которые еще не были отправлены (где ProcessedOnUtc == null)
        var outboxMessages = await _dbContext
            .Set<OutboxMessage>()
            .Where(m => m.HandledAtUtc == null)
            .OrderBy(o => o.OccuredAtUtc)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        // Если такие есть, то перебираем их в цикле
        if (outboxMessages.Any())
        {
            foreach (var outboxMessage in outboxMessages)
            {
                // Десериализуем в объект
                var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(outboxMessage.Content,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

                // Отправляем, и только если отправка была успешна - ставим дату отправки
                await _publisher.Publish(domainEvent, context.CancellationToken);
                outboxMessage.HandledAtUtc = DateTime.UtcNow;
            }

            // ?: if one of the event failed we interrupt entire chain, and in the next iteration even success event will be repeated
            // ?: if job is not able to handle all events during an iteration there might be event handles that already started but not finished, and again they will be repeated.
            
            // Сохраняем 
            await _dbContext.SaveChangesAsync();
        }
    }
}