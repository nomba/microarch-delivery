using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

/*
 Бизнес-правила Courier:

    Courier - это курьер, он состоит из:
        Id (uuid, идентификатор)
        Name (string, имя курьера)
        Transport (Transport, транспорт курьера)
        Location (Location, местоположение курьера)
        Status (CourierStatus, статус курьера)
    Курьер может быть создан:
        При создании нужно передать Name , Transport, Location
        При создании курьер свободен (CourierStatus = Free)
    Курьеру можно установить статус Busy, если он назначен на заказ
    Курьеру можно установить статус Free , если он завершил и больше не назначен на заказ
    Курьер должен уметь возвращать количество шагов, которое он потенциально затратит на путь до локации заказа. При расчете нужно участь скорость транспорта курьера. К примеру:
        Есть курьер на велосипеде
        Курьер находится в точке (1,1)
        Заказ находится в точке (5,5)
        Курьеру надо пройти 4 клетки по горизонтали и 4 по вертикали, чтобы оказаться в точке доставки заказа. Суммарная дистанция равна - 8 клеток
        Транспорт у курьера "Велосипед" и он едет со скоростью 2 клетки за 1 шаг
        Итого - курьеру нужно 4 шага, чтобы доставить заказ
    Курьер может переместиться на один шаг в сторону Location заказа:
        Размер шага курьера равен скорости его транспорта, к примеру, если скорость велосипеда = 2, это значит, что шаг курьера на велосипеде = 2 клетки
        Курьер может ходить как горизонтально, так и вертикально, но не наискосок. К примеру, если шаг курьера =2 клетки, то он может сделать 1 шаг по горизонтали и 1 шаг по вертикали или по 2 шага по прямой.
        Если транспорт курьера движется, к примеру, со скоростью 4 клетки за 1 шаг, а заказ находится ближе, к примеру, в 2 клетках, то курьер должен переместиться только до Location заказа
 */

public class Courier : Aggregate
{
    [ExcludeFromCodeCoverage]
    private Courier()
    {
    }
    
    private Courier(Guid courierId, string name, Transport transport, Location location) : this()
    {
        Id = courierId;
        Name = name;
        Transport = transport;
        Location = location;
        Status = CourierStatus.Free;
    } 
    
    public string Name { get; }
    public Transport Transport { get; }
    public Location Location { get; private set; }
    public CourierStatus Status { get; private set; }
    public Guid? OrderId { get; private set; }
    public Location OrderLocation { get; private set; }

    public static Result<Courier, Error> Create(string name, Transport transport, Location location)
    {
        if (string.IsNullOrWhiteSpace(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        if (transport == null) return GeneralErrors.ValueIsRequired(nameof(transport));
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        return new Courier(Guid.NewGuid(), name, transport, location);
    }
    
    public Result<DeliveryTime, Error> CheckDeliveryTime(Order order)
    {
        if (order == null) return GeneralErrors.ValueIsRequired(nameof(order));
        
        return DeliveryTime.Calculate(Location, order.Location, Transport.Speed);
    }
    
    public UnitResult<Error> Assign(Order order)
    {
        if (order == null) return GeneralErrors.ValueIsRequired(nameof(order));
        if (Status != CourierStatus.Free) return Errors.CantAssignOrderToBusyCourier(Id);
            
        OrderId = order.Id;
        OrderLocation = order.Location;
        Status = CourierStatus.Busy;
        return UnitResult.Success<Error>();
    }
    
    public UnitResult<Error> Move()
    {
        if (Status == CourierStatus.Free) return Errors.CantMoveFreeCourier(Id);
        
        // Высчитываем новое положения курьера в с учетом его транспорта
        
        var x = Location.X;
        var y =  Location.Y;

        for (int stepCount = 0; stepCount < Transport.Speed.Value; stepCount++)
        {
            var xDiff = OrderLocation.X - x;
            var yDiff = OrderLocation.Y - y;
            
            if (xDiff != 0)
            {
                x += xDiff > 0 ? 1 : -1;
                continue;
            }
            
            if (yDiff != 0)
            {
                y += yDiff > 0 ? 1 : -1;
            }
        }
        
        var newLocation = Location.Create(x, y);
        
        if (newLocation.IsFailure)
            return newLocation.Error;
        
        // Перемещаем курьера
        
        Location = newLocation.Value;
            
        // Если курьер доставил заказ (=дошёл до точки), то в рамках этого же Use Case мы завершаем заказ (Completed), а курьер снова свободен (Ready)
        
        if (Location == OrderLocation)
            Status = CourierStatus.Free;
        
        return UnitResult.Success<Error>();
    }
    
    [ExcludeFromCodeCoverage]
    public class Errors
    {
        public static Error CantAssignOrderToBusyCourier(Guid courierId)
        {
            return new Error($"{nameof(Courier).ToLowerInvariant()}.cant.assign.order.to.busy.courier",
                $"Нельзя назначить заказ на курьера, который занят. Id курьера = {courierId}");
        }
        
        public static Error CantMoveFreeCourier(Guid courierId)
        {
            return new Error($"{nameof(Courier).ToLowerInvariant()}.cant.move.without.order",
                $"Нельзя переместить курьера без заказа. Id курьера = {courierId}");
        }
    }
}