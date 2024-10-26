using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Primitives;

namespace DeliveryApp.Core.Ports;

/*
    Реализуйте Repository для Courier, он должен содержать следующие методы:
        Добавить курьера
        Обновить курьера
        Получить курьера по идентификатору
        Получить всех свободных курьеров (курьеры со статусом "Ready")
 */

public interface ICourierRepository : IRepository<Courier>
{
    Task Add(Courier courier, CancellationToken cancellationToken = default);
    Task Update(Courier courier, CancellationToken cancellationToken = default); 
    Task<Courier> GetById(Guid courierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Courier>> GetAllFree(CancellationToken cancellationToken = default);
}