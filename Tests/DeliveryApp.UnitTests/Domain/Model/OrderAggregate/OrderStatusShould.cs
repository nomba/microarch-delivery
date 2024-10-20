using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.OrderAggregate;

public class OrderStatusShould
{
    public static IEnumerable<object[]> GetOrderStatuses()
    {
        yield return [OrderStatus.Created, "created"];
        yield return [OrderStatus.Assigned, "assigned"];
        yield return [OrderStatus.Completed, "completed"];
    }

    [Theory]
    [MemberData(nameof(GetOrderStatuses))]
    public void ReturnCorrectIdAndName(OrderStatus orderStatus, string name)
    {
        //Arrange

        //Act

        //Assert
        
        orderStatus.Name.Should().Be(name);
    }

    [Theory]
    [InlineData(1, "created")]
    [InlineData(2, "assigned")]
    [InlineData(3, "completed")]
    public void CanBeFoundById(int id, string name)
    {
        //Arrange

        //Act
        
        var status = OrderStatus.FromId(id).Value;

        //Assert
        
        status.Id.Should().Be(id);
        status.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("created")]
    [InlineData("assigned")]
    [InlineData("completed")]
    public void CanBeFoundByName(string name)
    {
        //Arrange

        //Act
        
        var result = OrderStatus.FromName(name);

        //Assert
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
    }

    [Fact]
    public void ReturnErrorWhenTransportNotFoundById()
    {
        //Arrange
        
        var id = -1;

        //Act
        
        var result = OrderStatus.FromId(id);

        //Assert
        
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ReturnErrorWhenStatusNotFoundByName()
    {
        //Arrange
        
        var name = "not-existed-order-status";

        //Act
        
        var result = OrderStatus.FromName(name);

        //Assert
        
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ReturnListOfStatuses()
    {
        //Arrange

        //Act
        
        var allStatuses = OrderStatus.List();

        //Assert
        
        allStatuses.Should().NotBeEmpty();
    }
}