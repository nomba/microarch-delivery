using BasketConfirmed;
using Confluent.Kafka;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DeliveryApp.Api.Adapters.Kafka.BasketConfirmed;

public class ConsumerService : BackgroundService
{
    private readonly string _basketConfirmedTopic;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ConsumerService(IServiceScopeFactory serviceScopeFactory, IOptions<Settings> settings)
    {
        // Cannot use an instance IMediator in singleton dep (BackgroundService) directly because command may require scoped dependencies like IRepository
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        
        if (string.IsNullOrWhiteSpace(settings.Value.MESSAGE_BROKER_HOST)) 
            throw new ArgumentNullException(nameof(Settings.MESSAGE_BROKER_HOST));
        
        if (string.IsNullOrWhiteSpace(settings.Value.BASKET_CONFIRMED_TOPIC)) 
            throw new ArgumentNullException(nameof(Settings.BASKET_CONFIRMED_TOPIC));
        
        _basketConfirmedTopic = settings.Value.BASKET_CONFIRMED_TOPIC;
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = settings.Value.MESSAGE_BROKER_HOST,
            GroupId = "DeliveryConsumerGroup",
            EnableAutoOffsetStore = false,
            EnableAutoCommit = true,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        };
        
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_basketConfirmedTopic);
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
                var consumeResult = _consumer.Consume(stoppingToken);

                if (consumeResult.IsPartitionEOF) 
                    continue;

                Console.WriteLine(
                    $"Received message at {consumeResult.TopicPartitionOffset}\n Key:{consumeResult.Message.Key}\n Value:{consumeResult.Message.Value}");
                var basketConfirmedIntegrationEvent =
                    JsonConvert.DeserializeObject<BasketConfirmedIntegrationEvent>(consumeResult.Message.Value);

                var createOrderCommand = new CreateOrderCommand(
                    Guid.Parse(basketConfirmedIntegrationEvent.BasketId),
                    basketConfirmedIntegrationEvent.Address.Street);

                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var scopedMediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                
                var response = await scopedMediator.Send(createOrderCommand, stoppingToken);

                if (!response)
                    Console.WriteLine("Handling command error");

                try
                {
                    _consumer.StoreOffset(consumeResult);
                }
                catch (KafkaException e)
                {
                    Console.WriteLine($"Store Offset error: {e.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _consumer.Close();
        }
    }
}

 