using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

/// <summary>
///     Статус
/// </summary>
public class OrderStatus : Entity<int>
{
    public static readonly OrderStatus Created = new(1, nameof(Created).ToLowerInvariant());
    public static readonly OrderStatus Assigned = new(2, nameof(Assigned).ToLowerInvariant());
    public static readonly OrderStatus Completed = new(3, nameof(Completed).ToLowerInvariant());

    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private OrderStatus()
    {
    }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <param name="name">Название</param>
    private OrderStatus(int id, string name) : this()
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    ///     Название
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Список всех значений списка
    /// </summary>
    /// <returns>Значения списка</returns>
    public static IEnumerable<OrderStatus> List()
    {
        yield return Created;
        yield return Assigned;
        yield return Completed;
    }

    /// <summary>
    ///     Получить статус по названию
    /// </summary>
    /// <param name="name">Название</param>
    /// <returns>Статус</returns>
    public static Result<OrderStatus, Error> FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (state == null) return Errors.StatusIsWrong();
        return state;
    }

    /// <summary>
    ///     Получить статус по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <returns>Статус</returns>
    public static Result<OrderStatus, Error> FromId(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);
        if (state == null) return Errors.StatusIsWrong();
        return state;
    }

    /// <summary>
    ///     Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error StatusIsWrong()
        {
            return new Error($"{nameof(OrderStatus).ToLowerInvariant()}.is.wrong",
                $"Не верное значение. Допустимые значения: {nameof(OrderStatus).ToLowerInvariant()}: {string.Join(",", List().Select(s => s.Name))}");
        }
    }
}