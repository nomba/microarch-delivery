using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;

/*
    В реальной системе мы бы получали координаты от самих курьеров.
    В нашей же системе будет Job, который срабатывает каждую секунду. Сам Job мы сделаем в 7 модуле. При срабатывании Job будет вызываться этот Use Case.
    Поэтому нам нужно реализовать UseCase, который смещает всех курьеров на 1 шаг в сторону заказа со скоростью их транспорта.
    А если курьер доставил заказ (координата курьера и заказа совпадают), то в рамках этого же Use Case мы завершаем заказ (переводим в Completed), а курьер снова свободен (переводим в Free)
 */

public class MoveCouriersCommandHandler : IRequestHandler<MoveCouriersCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICourierRepository _courierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MoveCouriersCommandHandler(IOrderRepository orderRepository, ICourierRepository courierRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _courierRepository = courierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(MoveCouriersCommand request, CancellationToken cancellationToken)
    {
        // Забираем все назначенные заказы, чтобы подвигать их курьеров
        var assignedOrders = await _orderRepository.GetAllAssigned(cancellationToken);

        foreach (var assignedOrder in assignedOrders)
        {
            // Завершаем работы если обнаружен назначенный заказ без курьера
            // В идеале, такого не должно быть по доменной модели, перестраховываемся 
            
            if (assignedOrder.CourierId is not {} courierId)
                return false;

            var courier = await _courierRepository.GetById(courierId, cancellationToken);
            
            var moveResult = courier.Move();
            if (moveResult.IsFailure)
                return false;
            
            await _courierRepository.Update(courier, cancellationToken);
            
            if (courier.Status != CourierStatus.Free) 
                continue;
            
            // Двигаем курьера к текущему заказу пока он не освободиться
            // После завершаем заказ

            var completeResult = assignedOrder.Complete();
            if (completeResult.IsFailure)
                return false;
            
            await _orderRepository.Update(assignedOrder, cancellationToken);
        }
        
        return await _unitOfWork.SaveEntities(cancellationToken);
    }
}