namespace DeliveryApp.Core.Application.UseCases.Queries.GetIncompleteOrders;

public class GetIncompleteOrdersResponse
{
    public GetIncompleteOrdersResponse(IReadOnlyCollection<Order> orders)
    {
        Orders = orders;
    }

    public IReadOnlyCollection<Order> Orders { get; }
}

public class Order
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Геопозиция (X,Y)
    /// </summary>
    public Location Location { get; set; }
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