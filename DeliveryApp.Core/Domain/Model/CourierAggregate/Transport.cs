using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

/*
Бизнес-правила Transport:
    Transport - это вид транспорта курьера, он состоит из:
        Id (integer, идентификатор)
        Name (string, название транспорта)
        Speed (integer, скорость)
    Transport бывает 3 видов:
        Pedestrian (пешеход), у которого Speed=1
        Bicycle (велосипедист), у которого Speed=2
        Car (автомобиль), у которого Speed=3
    Должна быть возможность
        Получить транспорт по названию
        Получить транспорт по идентификатору
    Два транспорта должны быть равны, если их ID равны 
*/

// Исходя из бизнес-правил, фактически, мы оперируем НЕ транспортом, А определенном его типом (Велоспипед, Пешеход, Авто).
// Так как физически в системе доставки мы можем иметь несколько автомобилей, несколько великов у кажого из транспортов свой номер, свои особенности
// То в дальнейшем доменная модель (например, собственная база авто/великов) может обрести новые инварианты, которые завязываются на конкретный автомобиль/велик
// Но в целях урощения, для текущих задач, считаем, что нам не нужны конкретные велики/авто,а целом важен только тип.

public class Transport : Entity<int>
{
    public static readonly Transport Pedestrian = new(1, nameof(Pedestrian).ToLowerInvariant(), Speed.Slow);
    public static readonly Transport Bicycle = new(2, nameof(Bicycle).ToLowerInvariant(), Speed.Moderate);
    public static readonly Transport Car  = new(3, nameof(Car).ToLowerInvariant(), Speed.Fast);
        
    [ExcludeFromCodeCoverage]
    private Transport()
    {
    }
    
    private Transport(int id, string name, Speed speed) : this()
    {
        Id = id;
        Name = name;
        Speed = speed;
    }

    public string Name { get;  }
    public Speed Speed { get; }
    
    public static IEnumerable<Transport> List()
    {
        yield return Pedestrian;
        yield return Bicycle;
        yield return Car;
    }
    
    public static Result<Transport, Error> FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (state == null) return Errors.TransportTypeIsWrong();
        return state;
    }
    
    public static Result<Transport, Error> FromId(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);
        if (state == null) return Errors.TransportTypeIsWrong();
        return state;
    }
    
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error TransportTypeIsWrong()
        {
            return new Error($"{nameof(Transport).ToLowerInvariant()}.is.wrong",
                $"Не верное значение. Допустимые значения: {nameof(Transport).ToLowerInvariant()}: {string.Join(",", List().Select(s => s.Name))}");
        }
    }
}