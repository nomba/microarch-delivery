namespace DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers;

public class GetBusyCouriersResponse
{
    public GetBusyCouriersResponse(IReadOnlyCollection<Courier> couriers)
    {
        Couriers = couriers;
    }

    public IReadOnlyCollection<Courier> Couriers { get; }
}

public class Courier
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Имя
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Геопозиция (X,Y)
    /// </summary>
    public Location Location { get; set; }

    /// <summary>
    ///     Вид транспорта
    /// </summary>
    public int TransportId { get; set; }
}

public class Location
{
    /// <summary>
    ///     Горизонталь
    /// </summary>
    public int X { get; set; }

    /// <summary>
    ///     Вертикаль
    /// </summary>
    public int Y { get; set; }
}