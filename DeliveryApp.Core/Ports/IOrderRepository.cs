using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Ports;

/*
    Реализуйте Repository для Order, он должен содержать следующие методы:
        Добавить заказ
        Обновить заказ
        Получить заказ по идентификатору
        Получить все новые заказы (заказы со статусом "Created")
        Получить все назначенные заказы (заказы со статусом "Assigned")
 */

public interface IOrderRepository : IRepository<Order>
{
    Task Add(Order order, CancellationToken cancellationToken = default);
    Task Update(Order order, CancellationToken cancellationToken = default);
    Task<Order> GetById(Guid orderId, CancellationToken cancellationToken = default);
    
    // Should avoid generic method, like `GetByStatus`. Because it leads to inconsistency and abstraction leak
    // e.g. there already is `GetByStatus` and the new requirement occurs: "get all assigned order on current week or previous year or whatever"
    // There will be a dilemma:
    //  - introduce a new special method `GetAllCreatedOnThisWeek` to explicitly express intention, have more control and optimization possibility 
    //  or
    //  - extend current `GetByStatus` with time argument, have "the one" method, but modify existing implementation and blur the purpose it.  
    
    Task<IReadOnlyCollection<Order>> GetAllCreated(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Order>> GetAllAssigned(CancellationToken cancellationToken = default);
}