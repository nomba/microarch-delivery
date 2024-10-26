using System;
using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Services.DispatchService;
using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Services;

public class DispatchServiceShould
{
    [Fact]
    public void FindBestMatchCourierToOrder()
    {
        // Arrange
        
        var dispatchService = new DispatchService();
        var order = Order.Create(Guid.NewGuid(), Location.Min).Value;
        var courier1 = Courier.Create("courier1", Transport.Pedestrian, Location.Max).Value;
        var courier2 = Courier.Create("courier2", Transport.Bicycle, Location.Max).Value;
        var best = Courier.Create("best", Transport.Car, Location.Max).Value;

        // Act

        var result = dispatchService.Dispatch(order, new[] {courier1, courier2, best});

        // Assert
        
        result.IsSuccess.Should().BeTrue();
        order.CourierId.Should().Be(best.Id);
    }
}