using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.SharedKernel;

public class Speed : ValueObject
{
    public static readonly Speed Slow = new(1); 
    public static readonly Speed Moderate = new(2); 
    public static readonly Speed Fast = new(3); 
    
    [ExcludeFromCodeCoverage]
    private Speed()
    {
        
    }

    private Speed(int value)
    {
        Value = value;
    }

    public int Value { get; }
    
    public static Result<Speed, Error> Create(int value)
    {
        if (value <= 0)
            return GeneralErrors.ValueIsInvalid(nameof(value));

        return new Speed(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}