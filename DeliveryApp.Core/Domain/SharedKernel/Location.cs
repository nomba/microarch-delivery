﻿using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.SharedKernel;

/*
 Бизнес-правила Location:
    Location - это координата на доске, она состоит из X (горизонталь) и Y (вертикаль)
    Минимально возможная для установки координата 1,1
    Максимально возможная для установки координата 10,10
    2 координаты равны, если их X и Y равны, обеспечьте функционал проверки на эквивалентность
    Нельзя изменять объект Location после создания
    Должна быть возможность рассчитать расстояние между двумя Location. Расстояние между Location - это совокупное количество шагов по X и Y, которое необходимо сделать курьеру, чтобы достигнуть точки. На картинке ниже: расстояние между курьером и заказом - 2 шага по X и 3 шага по Y, суммарно 5 шагов.
    Должна быть возможность создать рандомную координату. В будущем эта функциональность будет использована в целях тестирования
*/

public class Location : ValueObject
{
    private const int MinX = 1;
    private const int MaxX = 10;
    private const int MinY = 1;
    private const int MaxY = 10;

    [ExcludeFromCodeCoverage]
    private Location()
    {
        
    }
    
    private Location(int x, int y)
    {
        X = x;
        Y = y;
    }
        
    /// <summary>
    /// Horizontal coordinate
    /// </summary>
    public int X { get; }
    
    /// <summary>
    /// Vertical coordinate
    /// </summary>
    public int Y { get; }

    public static Result<Location, Error> Create(int x, int y)
    {
        if (x is < MinX or > MaxY)
            return GeneralErrors.ValueIsInvalid(nameof(x));

        if (y is < MinY or > MaxY)
            return GeneralErrors.ValueIsInvalid(nameof(y));

        return new Location(x, y);
    }

    public static Location RollDice()
    {
        var rnd = new Random();
        return new Location(rnd.Next(MinX, MaxX + 1), rnd.Next(MinY, MaxY + 1));
    }

    public Distance GetDistanceTo(Location another)
    {
        var distanceValue = Math.Abs(X - another.X) + Math.Abs(Y - another.Y);
        return Distance.Create(distanceValue).Value;
    }

    [ExcludeFromCodeCoverage]
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}