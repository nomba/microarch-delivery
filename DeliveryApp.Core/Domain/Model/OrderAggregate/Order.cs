using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

/// <summary>
///     Заказ
/// </summary>
public class Order : Aggregate
{
    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private Order()
    {
    }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <param name="location">Геопозиция</param>
    private Order(Guid orderId, Location location) : this()
    {
        Id = orderId;
        Location = location;
        Status = OrderStatus.Created;
    }

    /// <summary>
    ///     Идентификатор исполнителя (курьера)
    /// </summary>
    public Guid? CourierId { get; private set; }

    /// <summary>
    ///     Местоположение, куда нужно доставить заказ
    /// </summary>
    public Location Location { get; private set; }

    /// <summary>
    ///     Статус
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <param name="location">Геопозиция</param>
    /// <returns>Результат</returns>
    public static Result<Order, Error> Create(Guid orderId, Location location)
    {
        if (orderId == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(orderId));
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));
        return new Order(orderId, location);
    }

    /// <summary>
    ///     Назначить заказ на курьера
    /// </summary>
    /// <param name="courier">Курьер</param>
    /// <returns>Результат</returns>
    public UnitResult<Error> Assign(Courier courier)
    {
        if (courier == null) return GeneralErrors.ValueIsRequired(nameof(courier));
        
        CourierId = courier.Id;
        Status = OrderStatus.Assigned;

        RaiseDomainEvent(new OrderAssignedDomainEvent(Id));
        return UnitResult.Success<Error>();
    }

    /// <summary>
    ///     Завершить выполнение заказа
    /// </summary>
    /// <returns>Результат</returns>
    public UnitResult<Error> Complete()
    {
        if (Status != OrderStatus.Assigned) return Errors.CantCompletedNotAssignedOrder();

        Status = OrderStatus.Completed;
        
        RaiseDomainEvent(new OrderCompletedDomainEvent(Id));
        return UnitResult.Success<Error>();
    }

    /// <summary>
    ///     Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error CantCompletedNotAssignedOrder()
        {
            return new Error($"{nameof(Order).ToLowerInvariant()}.cant.completed.not.assigned.order",
                "Нельзя завершить заказ, который не был назначен");
        }
    }
}