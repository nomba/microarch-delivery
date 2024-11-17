using Confluent.Kafka;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using Newtonsoft.Json;
using Primitives;
using OrderStatusChanged;

namespace DeliveryApp.Infrastructure.Adapters.Kafka.OrderStatusChanged;

// It's wierd that this class is located in OrderStatusChanged because it can handle any type of integration events
// This solution takes from the lesson
public class Producer : IMessageBusProducer
{
    private readonly string _orderStatusChangedTopic;
    private readonly ProducerConfig _config;

    public Producer(string messageBrokerAddress, string orderStatusChangedTopic)
    {
        if (string.IsNullOrWhiteSpace(messageBrokerAddress))
            throw new ArgumentNullException(nameof(messageBrokerAddress));

        if (string.IsNullOrWhiteSpace(orderStatusChangedTopic))
            throw new ArgumentNullException(nameof(orderStatusChangedTopic));
        _orderStatusChangedTopic = orderStatusChangedTopic;

        _config = new ProducerConfig
        {
            BootstrapServers = messageBrokerAddress
        };
    }

    public async Task Publish(DomainEvent @event, CancellationToken cancellationToken = default)
    {
        var integrationEvent = MapDomainEventToIntegration(@event);

        var message = new Message<string, string>
        {
            Key = @event.EventId.ToString(),
            Value = JsonConvert.SerializeObject(integrationEvent)
        };

        using var producer = new ProducerBuilder<string, string>(_config).Build();
        await producer.ProduceAsync(_orderStatusChangedTopic, message, cancellationToken);
    }

    private static object MapDomainEventToIntegration(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            OrderAssignedDomainEvent orderAssignedDomainEvent => new OrderStatusChangedIntegrationEvent
            {
                OrderId = orderAssignedDomainEvent.OrderId.ToString(),
                OrderStatus = OrderStatus.Assigned
            },

            OrderCompletedDomainEvent orderCompletedDomainEvent => new OrderStatusChangedIntegrationEvent
            {
                OrderId = orderCompletedDomainEvent.OrderId.ToString(),
                OrderStatus = OrderStatus.Completed
            },

            _ => throw new InvalidOperationException($"The domain event {domainEvent.GetType().FullName} is not mapped.")
        };
    }
}