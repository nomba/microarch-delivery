using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.SharedKernel;

public class DistanceShould
{
    [Fact]
    public void BeValidWhenValuePositive()
    {
        var result = Distance.Create(1);
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void BeNoneWhenValueZero()
    {
        var zeroDistance = Distance.Create(0).Value;

        var result = zeroDistance == Distance.None;
        
        result.Should().BeTrue();
    }
    
    [Fact]
    public void EqualWithAnotherWhenValueTheSame()
    {
        var first = Distance.Create(1).Value;
        var second = Distance.Create(1).Value;

        var result = first == second;

        result.Should().BeTrue();
    }
    
    [Fact]
    public void EqualWithAnotherWhenValueDifferent()
    {
        var first = Distance.Create(1).Value;
        var second = Distance.Create(2).Value;

        var result = first == second;

        result.Should().BeFalse();
    }  
    
}