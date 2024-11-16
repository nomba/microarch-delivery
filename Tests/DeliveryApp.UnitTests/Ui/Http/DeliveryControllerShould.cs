using System.Threading.Tasks;
using DeliveryApp.Api.Adapters.Http;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace DeliveryApp.UnitTests.Ui.Http;

public class DeliveryControllerShould
{
    private readonly IMediator _mediator;

    // ReSharper disable once ConvertConstructorToMemberInitializers
    public DeliveryControllerShould()
    {
        _mediator = Substitute.For<IMediator>();
    }
    
    [Fact]
    public async Task ReturnOkWhenOrderCreated()
    {
        // Arrange

        _mediator.Send(Arg.Any<CreateOrderCommand>()).Returns(true);
        var deliveryController = new DeliveryController(_mediator);

        // Act

        var result = await deliveryController.CreateOrder();

        // Assert

        result.Should().BeOfType<OkResult>();
    }
    
    [Fact]
    public async Task ReturnConflictWhenOrderCreationFails()
    {
        // Arrange

        _mediator.Send(Arg.Any<CreateOrderCommand>()).Returns(false);
        var deliveryController = new DeliveryController(_mediator);

        // Act

        var result = await deliveryController.CreateOrder();

        // Assert

        result.Should().BeOfType<ConflictResult>();
    }
}