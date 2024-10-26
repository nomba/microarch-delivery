using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class CourierShould
{
    public static IEnumerable<object[]> GetIncorrectCourierParams()
    {
        yield return [null, Transport.Bicycle, Location.Min];
        yield return [" ", Transport.Bicycle, Location.Min];
        yield return ["Ваня", null, Location.Min];
        yield return ["Ваня", Transport.Bicycle, null];
    }
    
    [Fact]
    public void BeCorrectWhenParamsIsCorrect()
    {
        //Arrange

        const string name = "Ваня";
        var transport =  Transport.Bicycle;
        var location = Location.Min;

        //Act

        var result = Courier.Create(name, transport, location);

        //Assert
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Transport.Should().Be(transport);
        result.Value.Location.Should().Be(location);
    }

    [Theory]
    [MemberData(nameof(GetIncorrectCourierParams))]
    public void ReturnErrorWhenParamsIncorrect(string name, Transport transport, Location location)
    {
        //Arrange

        //Act

        var result = Courier.Create(name, transport, location);

        //Assert

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void HaveCorrectDeliveryTimeToOrder()
    {
        //Arrange
        
        var courierLocation = Location.Create(1, 1).Value;
        var orderLocation = Location.Create(5, 5).Value;
        var courier = Courier.Create("Ваня", Transport.Bicycle, courierLocation).Value;
        var order = Order.Create(Guid.NewGuid(), orderLocation).Value;

        //Act
        
        var result = courier.CheckDeliveryTime(order);

        //Assert
        
        result.IsSuccess.Should().BeTrue();
        result.Value.StepCount.Should().Be(4);
    }
    
    [Fact]
    public void BeAbleToAssignWhenFree()
    {
        //Arrange
        
        var courierLocation = Location.Create(1, 1).Value;
        var orderLocation = Location.Create(5, 5).Value;
        var courier = Courier.Create("Ваня", Transport.Bicycle, courierLocation).Value;
        var order = Order.Create(Guid.NewGuid(), orderLocation).Value;

        //Act
        
        var result = courier.Take(order);

        //Assert
        
        result.IsSuccess.Should().BeTrue();
        courier.Status.Should().Be(CourierStatus.Busy);
        courier.OrderId.Should().Be(order.Id);
    }
    
    [Fact]
    public void ReturnErrorOnReassigning()
    {
        //Arrange
        
        var courierLocation = Location.Create(1, 1).Value;
        var orderLocation = Location.Create(5, 5).Value;
        var courier = Courier.Create("Ваня", Transport.Bicycle, courierLocation).Value;
        var order = Order.Create(Guid.NewGuid(), orderLocation).Value;
        courier.Take(order);
        
        //Act
        
        var result = courier.Take(order);

        //Assert
        
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void ReachOrderLocationForSpecificTime()
    {
        //Arrange

        var expectedStepsCount = 4; // Time equivalent
        var initialCourierLocation = Location.Create(1, 1).Value;
        var orderLocation = Location.Create(5, 5).Value;
        var courier = Courier.Create("Ваня", Transport.Bicycle, initialCourierLocation).Value;
        var order = Order.Create(Guid.NewGuid(), orderLocation).Value;
        courier.Take(order);
        
        //Act

        var listResults = new List<UnitResult<Error>>();
        
        for (var i = 0; i < expectedStepsCount; i++)
        {
            var result = courier.Move();
            listResults.Add(result);
        }

        //Assert

        listResults.Should().OnlyContain(r => r.IsSuccess);
        courier.Status.Should().Be(CourierStatus.Free);
        courier.Location.Should().Be(orderLocation);
    }
    
    [Fact]
    public void ReachErrorWhenMoveFreeCourier()
    {
        //Arrange
        
        var courier = Courier.Create("Ваня", Transport.Bicycle, Location.Create(1, 1).Value).Value;
        
        //Act

        var result = courier.Move();
        
        //Assert

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
}