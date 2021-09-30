using System;
using MathCore.EF7.Interfaces.Entities;

namespace MathCore.EF7.Enities.Base
{
    /// <summary>Сущность, снабженная комментарием</summary>
    /// <typeparam name="TKey">Тип первичного ключа</typeparam>
    public abstract class DescriptedEntity<TKey> : Entity<TKey>, IDescriptedEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>Комментарий</summary>
        public string Description { get; set; }
    }

    /// <summary>Сущность, снабженная комментарием</summary>
    public abstract class DescriptedEntity : DescriptedEntity<int>, IDescriptedEntity { }
}
