using Dapper;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetIncompleteOrders;

/*
    Получить все незавершенные заказы (статус Created и Assigned)
    Мы можем отслеживать на карте заказы. Ответ данного Use Case должен содержать информацию о заказе и его местоположение.
 */

public class GetIncompleteOrdersQueryHandler : IRequestHandler<GetIncompleteOrdersQuery, GetIncompleteOrdersResponse>
{
    private readonly string _connectionString;

    public GetIncompleteOrdersQueryHandler(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }
    
    public async Task<GetIncompleteOrdersResponse> Handle(GetIncompleteOrdersQuery request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var commandResult = await connection.QueryAsync<dynamic>(
            "SELECT id, courier_id, location_x, location_y, status_id FROM orders WHERE status_id=@status_id",
            new {status_id = OrderStatus.Created.Id});
        
        var dynamicOrders = commandResult.AsList();
        return dynamicOrders.Count == 0 ? null : new GetIncompleteOrdersResponse(dynamicOrders.Select(item => MapToOrder(item)).Cast<Order>().ToArray());
    }
    
    private static Order MapToOrder(dynamic dynamicOrder)
    {
        var location = new Location { X = dynamicOrder.location_x, Y = dynamicOrder.location_y };
        return  new Order { Id = dynamicOrder.id, Location = location };;
    }
}