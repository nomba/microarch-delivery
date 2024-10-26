using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services.DispatchService;

public interface IDispatchService
{
    public UnitResult<Error> Dispatch(Order order, IReadOnlyCollection<Courier> couriers);
}