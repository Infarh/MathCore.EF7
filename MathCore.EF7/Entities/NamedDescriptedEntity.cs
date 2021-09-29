using System;
using MathCore.EF7.Interfaces.Entities;

namespace MathCore.EF7.Entities
{
    /// <summary>Именованная сущность с описанием</summary>
    /// <typeparam name="TKey">Тип первичного ключа</typeparam>
    public abstract class NamedDescriptedEntity<TKey> : NamedEntity<TKey>, IDescriptedEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>Описание</summary>
        public string Description { get; set; }
    }

    /// <inheritdoc cref="NamedDescriptedEntity{T}" />
    public abstract class NamedDescriptedEntity : NamedDescriptedEntity<int>, IDescriptedEntity { }
}
