using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.SharedKernel;

public class SpeedShould
{
    [Fact]
    public void BeValidWhenValuePositive()
    {
        //Arrange

        //Act
        
        var result = Speed.Create(1);
        
        //Assert
        
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void ReturnErrorWhenValueLessOrEqualZero()
    {
        //Arrange
        
        const int value = -1;

        
        
        var result = Speed.Create(value);

        //Assert
        
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void EqualWithAnotherWhenValueTheSame()
    {
        //Arrange
        
        //Act
        
        var first = Speed.Create(1).Value;
        var second = Speed.Create(1).Value;

        //Assert
        
        var result = first == second;
        result.Should().BeTrue();
    }
    
    [Fact]
    public void EqualWithAnotherWhenValueDifferent()
    {
        //Arrange
        
        //Act
        
        var first = Speed.Create(1).Value;
        var second = Speed.Create(2).Value;

        //Assert
        
        var result = first == second;
        result.Should().BeFalse();
    }

    [Fact]
    public void SlowMeanOnePoint()
    {
        //Arrange
        
        //Act
        
        //Assert
        
        Speed.Slow.Value.Should().Be(1);
    } 
    
    [Fact]
    public void ModerateMeanTwoPoint()
    {
        //Arrange
        
        //Act
        
        //Assert
        
        Speed.Moderate.Value.Should().Be(2);
    }
    
    [Fact]
    public void FastMeanThreePoint()
    {
        //Arrange
        
        //Act
        
        //Assert

        Speed.Fast.Value.Should().Be(3);
    }
}