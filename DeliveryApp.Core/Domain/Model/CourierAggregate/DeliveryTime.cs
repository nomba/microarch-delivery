using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.SharedKernel;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public class DeliveryTime : ValueObject
{
    [ExcludeFromCodeCoverage]
    private DeliveryTime()
    {
        
    }
    
    private DeliveryTime(int stepCount)
    {
        StepCount = stepCount;
    }
    
    public int StepCount { get; }
    
    public static DeliveryTime Calculate(Location current, Location target, Speed speed)
    {
        var distance = current.GetDistanceTo(target);
        
        // Integer division with round up
        return new DeliveryTime((distance.Value + speed.Value - 1) / speed.Value);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StepCount;
    }
}