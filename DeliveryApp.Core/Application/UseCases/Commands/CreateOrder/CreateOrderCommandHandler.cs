using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;

/*
    Сервис Delivery создаёт заказ, в результате оформления корзины.
    Сообщение "Корзина оформлена" будет приходить из Kafka, как только мы реализуем интеграцию с Basket.
    А пока нам достаточно реализовать Use Case создания заказа.
 */

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeoClient _geoClient;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CreateOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IGeoClient geoClient)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _geoClient = geoClient;
    }
    
    public async Task<bool> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Check if order is already created

        var existingOrder = await _orderRepository.GetById(request.BasketId, cancellationToken);
        if (existingOrder != null) 
            return true;

        var (_, isGeoFailure, location) = await _geoClient.GetLocation(request.Street, cancellationToken);
        
        if (isGeoFailure)
            return false;
        
        var (_, isFailure, newOrder) = Order.Create(request.BasketId, location);
        
        if (isFailure)
            return false;

        await _orderRepository.Add(newOrder, cancellationToken);
        return await _unitOfWork.SaveEntities(cancellationToken);
    }
}