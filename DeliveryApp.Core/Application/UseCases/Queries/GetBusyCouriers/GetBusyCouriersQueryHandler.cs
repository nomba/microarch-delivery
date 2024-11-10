using Dapper;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers;

/*
    Получить занятых курьеров (статус Busy)
    Мы можем отслеживать на карте курьеров, которые выполняют заказ. Ответ данного Use Case должен содержать информацию о курьере и его местоположение.
 */

public class GetBusyCouriersQueryHandler : IRequestHandler<GetBusyCouriersQuery, GetBusyCouriersResponse>
{
    private readonly string _connectionString;

    public GetBusyCouriersQueryHandler(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }
    
    public async Task<GetBusyCouriersResponse> Handle(GetBusyCouriersQuery request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var commandResult = await connection.QueryAsync<dynamic>(
            "SELECT id, name, transport_id, location_x, location_y, status_id FROM couriers WHERE status_id=@status_id",
            new {status_id = CourierStatus.Busy.Id});
        
        var dynamicCouriers = commandResult.AsList();
        return dynamicCouriers.Count == 0 ? null : new GetBusyCouriersResponse(dynamicCouriers.Select(item => MapToCourier(item)).Cast<Courier>().ToArray());
    }
    
    private static Courier MapToCourier(dynamic dynamicCourier)
    {
        var location = new Location { X = dynamicCourier.location_x, Y = dynamicCourier.location_y };
        return new Courier {Id = dynamicCourier.id, Name = dynamicCourier.name, TransportId = dynamicCourier.transport_id, Location = location};
    }
}