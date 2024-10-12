using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.SharedKernel;

public class Distance : ValueObject
{
    public static readonly Distance None = new(0);
    
    [ExcludeFromCodeCoverage]
    private Distance()
    {
        
    }

    private Distance(int value)
    {
        Value = value;
    }
    
    public int Value { get; }

    public static Result<Distance, Error> Create(int value)
    {
        if (value < 0)
            return GeneralErrors.ValueIsInvalid(nameof(value));

        return value == 0 ? None : new Distance(value);
    }
    
    [ExcludeFromCodeCoverage]
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}