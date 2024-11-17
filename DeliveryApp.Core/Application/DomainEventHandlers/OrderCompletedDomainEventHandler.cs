using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.DomainEventHandlers;

public class OrderCompletedDomainEventHandler : INotificationHandler<OrderCompletedDomainEvent>
{
    private readonly IMessageBusProducer _messageBusProducer;

    // ReSharper disable once ConvertToPrimaryConstructor
    public OrderCompletedDomainEventHandler(IMessageBusProducer messageBusProducer)
    {
        _messageBusProducer = messageBusProducer;
    }
    
    public Task Handle(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _messageBusProducer.Publish(notification, cancellationToken);
    }
}