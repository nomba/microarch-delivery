using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
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
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IGeoClient _geoClientMock;

    // ReSharper disable once ConvertConstructorToMemberInitializers
    public CreateOrderCommandShould()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _geoClientMock = Substitute.For<IGeoClient>();
    }

    [Fact]
    public async Task ReturnTrueWhenOrderAlreadyExists()
    {
        // Arrange
        
        var orderId = Guid.NewGuid();
        var alreadyCreatedOrder = Order.Create(orderId, Location.Create(1, 1).Value).Value;
        _orderRepositoryMock.GetById(Arg.Any<Guid>()).Returns(Task.FromResult(alreadyCreatedOrder));
        _geoClientMock.GetLocation(Arg.Any<string>()).Returns(Task.FromResult(Location.Create(1, 1)));
        
        // Act
        
        var command = new CreateOrderCommand(orderId, "Test Street");
        var handler = new CreateOrderCommandHandler(_orderRepositoryMock, _unitOfWorkMock, _geoClientMock);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        
        result.Should().BeTrue();
        await _orderRepositoryMock.Received(1).GetById(Arg.Is(orderId));
        await _orderRepositoryMock.Received(0).Add(Arg.Any<Order>());
        await _unitOfWorkMock.Received(0).SaveEntities();
        await _geoClientMock.Received(0).GetLocation(Arg.Any<string>());
    }

    
    [Fact]
    public async Task ReturnTrueWhenOrderCreated()
    {
        // Arrange
        
         const string street = "Test Street";
        _orderRepositoryMock.GetById(Arg.Any<Guid>()).Returns(Task.FromResult((Order) null));
        _unitOfWorkMock.SaveEntities().Returns(Task.FromResult(true));
        _geoClientMock.GetLocation(Arg.Any<string>()).Returns(Task.FromResult(Location.Create(1, 1)));
        
        // Act
        
        var command = new CreateOrderCommand(Guid.NewGuid(), street);
        var handler = new CreateOrderCommandHandler(_orderRepositoryMock, _unitOfWorkMock, _geoClientMock);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        
        result.Should().BeTrue();
        await _orderRepositoryMock.Received(1).Add(Arg.Any<Order>());
        await _unitOfWorkMock.Received(1).SaveEntities();
        await _geoClientMock.Received(1).GetLocation(street);
    }
}