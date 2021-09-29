using System;

namespace MathCore.EF7.Interfaces.Entities
{
    /// <summary>Сущность, определенная во времени</summary>
    /// <typeparam name="TKey">Тип первичного ключа</typeparam>
    public interface ITimedEntity<out TKey> : IEntity<TKey>
    {
        /// <summary>Время</summary>
        DateTimeOffset Time { get; }
    }

    /// <summary>Сущность, определенная во времени</summary>
    public interface ITimedEntity : ITimedEntity<int>, IEntity { }
}
