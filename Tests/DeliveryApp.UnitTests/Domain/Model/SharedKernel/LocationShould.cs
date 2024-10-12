using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.SharedKernel;

public class LocationShould
{
    [Theory]
    [InlineData(1,1)]
    [InlineData(1,10)]
    [InlineData(10,1)]
    [InlineData(10,10)]
    public void BeValidWhenCoordinatesCorrect(int x, int y)
    {
        var result = Location.Create(x, y);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1,1)]
    [InlineData(1,-1)]
    [InlineData(11,1)]
    [InlineData(1,11)]
    public void ReturnErrorWhenCoordinatesIncorrect(int x, int y)
    {
        var result = Location.Create(x, y);
        result.IsFailure.Should().BeTrue();
    }
    
    [Fact]
    public void EqualWithAnotherWhenCoordinatesTheSame()
    {
        var first = Location.Create(1, 1).Value;
        var second = Location.Create(1, 1).Value;

        var result = first == second;

        result.Should().BeTrue();
    }
    
    [Fact]
    public void NotEqualWithAnotherWhenCoordinatesDifferent()
    {
        var first = Location.Create(1, 1).Value;
        var second = Location.Create(10, 10).Value;

        var result = first == second;

        result.Should().BeFalse();
    }  
    
    [Fact]
    public void HaveNoDistanceBetweenWhenCoordinatesTheSame()
    {
        var first = Location.Create(1, 1).Value;
        var second = Location.Create(1, 1).Value;

        var distance = first.GetDistanceTo(second);

        distance.Should().Be(Distance.None);
    }
    
    [Fact]
    public void GetMaximumDistanceBetweenMaximumDistantCoordinates()
    {
        var first = Location.Create(1, 1).Value;
        var second = Location.Create(10, 10).Value;

        var distance = first.GetDistanceTo(second);

        distance.Value.Should().Be(18);
    }
    
    [Fact]
    public void GetCorrectDistanceWhenHorizontalTheSame()
    {
        var first = Location.Create(1, 1).Value;
        var second = Location.Create(1, 5).Value;

        var distance = first.GetDistanceTo(second);

        distance.Value.Should().Be(4);
    }
    
    [Fact]
    public void GetCorrectDistanceWhenVerticalTheSame()
    {
        var first = Location.Create(1, 5).Value;
        var second = Location.Create(5, 5).Value;

        var distance = first.GetDistanceTo(second);

        distance.Value.Should().Be(4);
    }
    
    [Fact]
    public void GetCorrectDistanceWhenHorizontalAndVerticalDifferent()
    {
        var first = Location.Create(1, 1).Value;
        var second = Location.Create(5, 5).Value;

        var distance = first.GetDistanceTo(second);

        distance.Value.Should().Be(8);
    }
    
    [Fact]
    public void DoRollDice()
    {
        var location = Location.RollDice();
        location.Should().NotBeNull();
    }
}