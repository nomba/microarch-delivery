using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class TransportShould
{
    public static IEnumerable<object[]> GetTransports()
    {
        yield return [Transport.Pedestrian, 1, "pedestrian", 1];
        yield return [Transport.Bicycle, 2, "bicycle", 2];
        yield return [Transport.Car, 3, "car", 3];
    }

    [Theory]
    [MemberData(nameof(GetTransports))]
    public void ReturnCorrectIdAndName(Transport transport, int id, string name, int speed)
    {
        //Arrange

        //Act

        //Assert

        transport.Id.Should().Be(id);
        transport.Name.Should().Be(name);
        transport.Speed.Value.Should().Be(speed);

    }

    [Theory]
    [MemberData(nameof(GetTransports))]
    public void CanBeFoundById(Transport _, int id, string name, int speed)
    {
        //Arrange

        //Act

        var transport = Transport.FromId(id).Value;

        //Assert

        transport.Id.Should().Be(id);
        transport.Name.Should().Be(name);
        transport.Speed.Value.Should().Be(speed);
    }

    [Theory]
    [MemberData(nameof(GetTransports))]
    public void CanBeFoundByName(Transport _, int id, string name, int speed)
    {
        //Arrange

        //Act
        
        var transport = Transport.FromName(name).Value;

        //Assert
        
        transport.Id.Should().Be(id);
        transport.Name.Should().Be(name);
        transport.Speed.Value.Should().Be(speed);
    }

    [Fact]
    public void ReturnErrorWhenTransportNotFoundById()
    {
        //Arrange
        
        var id = -1;

        //Act
        
        var result = Transport.FromId(id);

        //Assert
        
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ReturnErrorWhenTransportNotFoundByName()
    {
        //Arrange
        
        var name = "not-existed-transport";

        //Act
        
        var result = Transport.FromName(name);

        //Assert
        
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ReturnListOfStatuses()
    {
        //Arrange

        //Act
        
        var allTransports = Transport.List();

        //Assert
        
        allTransports.Should().NotBeEmpty();
    }
}