using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<bool>
{
    public CreateOrderCommand(Guid basketId, string street)
    {
        ArgumentNullException.ThrowIfNull(basketId);
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        
        BasketId = basketId;
        Street = street;
    }
    
    /// <summary>
    /// Идентификатор корзины
    /// </summary>
    /// <remarks>Id корзины берется за основу при создании Id заказа, они совпадают</remarks>
    public Guid BasketId { get; }

    /// <summary>
    /// Улица
    /// </summary>
    public string Street { get; }
}