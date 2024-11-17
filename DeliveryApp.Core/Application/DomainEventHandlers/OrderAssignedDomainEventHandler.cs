using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.DomainEventHandlers;

public class OrderAssignedDomainEventHandler : INotificationHandler<OrderAssignedDomainEvent>
{
    private readonly IMessageBusProducer _messageBusProducer;

    // ReSharper disable once ConvertToPrimaryConstructor
    public OrderAssignedDomainEventHandler(IMessageBusProducer messageBusProducer)
    {
        _messageBusProducer = messageBusProducer;
    }
    
    public Task Handle(OrderAssignedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _messageBusProducer.Publish(notification, cancellationToken);
    }
}