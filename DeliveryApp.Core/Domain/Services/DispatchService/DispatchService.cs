using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services.DispatchService;

/*
    Система сама распределяет заказы на курьеров, она берёт любой заказ в статусе Created (не распределённый) и ищет самого подходящего курьера.

    Алгоритм работы:
        За 1 раз мы диспетчеризуем только 1 заказ
        Логика диспетчеризации:
            Берём 1 заказ со статусом Created
            Берём всех курьеров в статусе Free
            Считаем время доставки заказа для каждого курьера, учитывая его текущее местоположение
            Побеждает курьер, который потенциально быстрее всего доставит заказ, его и назначаем на заказ
            
    Допущения:
        Считаем, что посылка (заказ) появляется у курьера сразу после назначения, ему не надо ехать на склад и потом к клиенту.
        Курьер начинает доставку из его текущего Location и завершает в Location заказа
 */

public class DispatchService : IDispatchService
{
    public UnitResult<Error> Dispatch(Order order, IReadOnlyCollection<Courier> couriers)
    {
        if (order is null) return GeneralErrors.ValueIsRequired(nameof(order));
        if (couriers == null) return GeneralErrors.ValueIsRequired(nameof(couriers));

        if (order.Status != OrderStatus.Created)
            return Errors.OrderIsAlreadyAssignedOrFinished(order.Id);
        
        // Выбираем подходящего курьера в соотвествии с алгоритмом

        var fastestCourier = couriers.MinBy(courier => courier.CheckDeliveryTime(order).Value.StepCount);

        if (fastestCourier is null)
            return Errors.CantFindAnyCourierForOrder(order.Id);
        
        // Назначаем курьера на заказ
        
        var assignOrderResult = order.Assign(fastestCourier);
        if (assignOrderResult.IsFailure)
            return assignOrderResult;

        // Резервируем курьера для заказа
        
        var takeOrderResult =  fastestCourier.Take(order);
        if (takeOrderResult.IsFailure)
            return takeOrderResult;
        
        return UnitResult.Success<Error>();
    }
    
    [ExcludeFromCodeCoverage]
    public class Errors
    {
        public static Error OrderIsAlreadyAssignedOrFinished(Guid orderId)
        {
            return new Error($"{nameof(DispatchService).ToLowerInvariant()}.order.is.already.assigned.or.finished",
                $"Заказ уже назначен или завершен. Id заказа = {orderId}");
        }
        
        public static Error CantFindAnyCourierForOrder(Guid orderId)
        {
            return new Error($"{nameof(DispatchService).ToLowerInvariant()}.cant.find.any.courier.for.order",
                $"Курьер для заказа не найден. Id заказа = {orderId}");
        }
    }
}