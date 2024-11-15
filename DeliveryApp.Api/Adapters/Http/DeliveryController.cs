using DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Controllers;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetIncompleteOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Courier = DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Models.Courier;
using Location = DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Models.Location;
using Order = DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Models.Order;



namespace DeliveryApp.Api.Adapters.Http;

public class DeliveryController : DefaultApiController
{
    private readonly IMediator _mediator;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeliveryController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public override async Task<IActionResult> CreateOrder()
    {
        var orderId = Guid.NewGuid();
        const string street = "Несуществующая";
        
        var createOrderCommand = new CreateOrderCommand(orderId, street);
        var response = await _mediator.Send(createOrderCommand);
        
        if (response) 
            return Ok();
        
        return Conflict();
    }

    public override async Task<IActionResult> GetCouriers()
    {
        // Task mismatches to API
        // According to the task only busy couriers are returned but API requires all couriers
        
        var getAllCouriersQuery = new GetBusyCouriersQuery();
        var response = await _mediator.Send(getAllCouriersQuery);
        
        if (response == null) 
            return NotFound();
        
        var model = response.Couriers.Select(c => new Courier
        {
            Id = c.Id,
            Name = c.Name,
            Location = new Location { X = c.Location.X, Y = c.Location.Y }
        });
        
        return Ok(model);
    }

    public override async Task<IActionResult> GetOrders()
    {
        var getIncompleteOrders = new GetIncompleteOrdersQuery();
        var response = await _mediator.Send(getIncompleteOrders);
        
        if (response == null) 
            return NotFound();
        
        var model = response.Orders.Select(o => new Order
        {
            Id = o.Id,
            Location = new Location { X = o.Location.X, Y = o.Location.Y }
        });
        
        return Ok(model);
    }
}