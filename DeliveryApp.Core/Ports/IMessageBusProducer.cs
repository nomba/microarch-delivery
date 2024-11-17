using Primitives;

namespace DeliveryApp.Core.Ports;

// In the lesson the producer has separate Publish method for each domain event
// Here, only one abstract method is used, I can't imagine the difference between publishing different domain events
public interface IMessageBusProducer
{
    Task Publish(DomainEvent @event, CancellationToken cancellationToken = default);
}