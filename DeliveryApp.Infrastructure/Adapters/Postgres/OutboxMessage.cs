namespace DeliveryApp.Infrastructure.Adapters.Postgres;

/// <summary>
///     Outbox
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>
    ///     Уникальный идентификатор сообщения
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Тип сообщения
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    ///     Тело сообщения (полезная информация)
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    ///     Дата создания
    /// </summary>
    public DateTime OccuredAtUtc { get; init; }

    /// <summary>
    ///     Дата публикации
    /// </summary>
    public DateTime? HandledAtUtc { get; init; }
}