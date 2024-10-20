using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class CourierStatusShould
{
    public static IEnumerable<object[]> GetCourierStatuses()
    {
        yield return [CourierStatus.Free, "free"];
        yield return [CourierStatus.Busy, "busy"];
    }

    [Theory]
    [MemberData(nameof(GetCourierStatuses))]
    public void ReturnCorrectIdAndName(CourierStatus courierStatus, string name)
    {
        //Arrange

        //Act

        //Assert
        
        courierStatus.Name.Should().Be(name);
    }

    [Theory]
    [InlineData(1, "free")]
    [InlineData(2, "busy")]
    public void CanBeFoundById(int id, string name)
    {
        //Arrange

        //Act
        
        var status = CourierStatus.FromId(id).Value;

        //Assert
        
        status.Id.Should().Be(id);
        status.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("free")]
    [InlineData("busy")]
    public void CanBeFoundByName(string name)
    {
        //Arrange

        //Act
        
        var courierStatus = CourierStatus.FromName(name).Value;

        //Assert
        
        courierStatus.Name.Should().Be(name);
    }

    [Fact]
    public void ReturnErrorWhenCourierStatusNotFoundById()
    {
        //Arrange
        
        var id = -1;

        //Act
        
        var result = CourierStatus.FromId(id);

        //Assert
        
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ReturnErrorWhenCourierStatusNotFoundByName()
    {
        //Arrange
        
        var name = "not-existed-courier-status";

        //Act
        
        var result = CourierStatus.FromName(name);

        //Assert
        
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ReturnListOfStatuses()
    {
        //Arrange

        //Act
        
        var allStatuses = CourierStatus.List();

        //Assert
        
        allStatuses.Should().NotBeEmpty();
    }
}