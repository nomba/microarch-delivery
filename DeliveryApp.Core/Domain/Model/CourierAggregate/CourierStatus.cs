using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

/// <summary>
///     Статус
/// </summary>
public class CourierStatus : Entity<int>
{
    public static readonly CourierStatus Free = new(1, nameof(Free).ToLowerInvariant());
    public static readonly CourierStatus Busy = new(2, nameof(Busy).ToLowerInvariant());

    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private CourierStatus()
    {
    }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <param name="name">Название</param>
    private CourierStatus(int id, string name) : this()
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
    public static IEnumerable<CourierStatus> List()
    {
        yield return Free;
        yield return Busy;
    }

    /// <summary>
    ///     Получить статус по названию
    /// </summary>
    /// <param name="name">Название</param>
    /// <returns>Статус</returns>
    public static Result<CourierStatus, Error> FromName(string name)
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
    public static Result<CourierStatus, Error> FromId(int id)
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
            return new Error($"{nameof(CourierStatus).ToLowerInvariant()}.is.wrong",
                $"Не верное значение. Допустимые значения: {nameof(CourierStatus).ToLowerInvariant()}: {string.Join(",", List().Select(s => s.Name))}");
        }
    }
}