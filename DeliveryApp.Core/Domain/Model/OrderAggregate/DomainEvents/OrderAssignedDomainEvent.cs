using Primitives;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;

public record OrderAssignedDomainEvent(Guid OrderId) : DomainEvent;