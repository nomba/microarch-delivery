using System;
using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application;

public class CreateOrderCommandShould
{
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateOrderCommandShould()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
    }

    [Fact]
    public async Task ReturnTrueWhenOrderAlreadyExists()
    {
        // Arrange
        
        var orderId = Guid.NewGuid();
        var alreadyCreatedOrder = Order.Create(orderId, Location.Create(1, 1).Value).Value;
        _orderRepositoryMock.GetById(Arg.Any<Guid>()).Returns(Task.FromResult(alreadyCreatedOrder));
        
        // Act
        
        var command = new CreateOrderCommand(orderId, "Test Street");
        var handler = new CreateOrderCommandHandler(_orderRepositoryMock, _unitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        
        result.Should().BeTrue();
        await _orderRepositoryMock.Received(1).GetById(Arg.Is(orderId));
        await _orderRepositoryMock.Received(0).Add(Arg.Any<Order>());
        await _unitOfWork.Received(0).SaveEntities();
    }

    
    [Fact]
    public async Task ReturnTrueWhenOrderCreated()
    {
        // Arrange
        
        _orderRepositoryMock.GetById(Arg.Any<Guid>()).Returns(Task.FromResult((Order) null));
        _unitOfWork.SaveEntities().Returns(Task.FromResult(true));

        // Act
        
        var command = new CreateOrderCommand(Guid.NewGuid(), "Test Street");
        var handler = new CreateOrderCommandHandler(_orderRepositoryMock, _unitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        
        result.Should().BeTrue();
        await _orderRepositoryMock.Received(1).Add(Arg.Any<Order>());
        await _unitOfWork.Received(1).SaveEntities();
    }
}