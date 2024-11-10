using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Services.DispatchService;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrders;

/*
    Система сама распределяет заказы, она берёт первый неназначенный заказ и ищет самого подходящего курьера.
    Алгоритм назначения описан ниже в разделе "Скоринг курьеров".
    Этот Use Case мы будем запускать с помощью Job в 7 модуле. А пока нам важно просто его реализовать.
    В этом Use Case мы должны получить свободных курьеров и неназначенные заказы и передать их алгоритму скоринга. Заказ и назначенного Курьера мы сохраняем в БД одной транзакцией.
 */

public class AssignOrdersCommandHandler : IRequestHandler<AssignOrdersCommand, bool>
{
    private readonly IDispatchService _dispatchService;
    private readonly IOrderRepository _orderRepository;
    private readonly ICourierRepository _courierRepository;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public AssignOrdersCommandHandler(IDispatchService dispatchService, IOrderRepository orderRepository, ICourierRepository courierRepository, IUnitOfWork unitOfWork)
    {
        _dispatchService = dispatchService;
        _orderRepository = orderRepository;
        _courierRepository = courierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(AssignOrdersCommand request, CancellationToken cancellationToken)
    {
        var newOrders = await _orderRepository.GetAllCreated(cancellationToken);
        
        if (newOrders.FirstOrDefault() is not { } firstNewOrder)
            return false;

        var freeCouriers = await _courierRepository.GetAllFree(cancellationToken);
        if (freeCouriers.Count == 0) 
            return false;
        
        var (_, isFailure, selectedCourier) = _dispatchService.Dispatch(firstNewOrder, freeCouriers);
        if (isFailure)
            return false;

        await _orderRepository.Update(firstNewOrder, cancellationToken);
        await _courierRepository.Update(selectedCourier, cancellationToken);
        return await _unitOfWork.SaveEntities(cancellationToken);
    }
}