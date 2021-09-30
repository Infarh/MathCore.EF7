using System;
using Microsoft.EntityFrameworkCore;

namespace MathCore.EF7.Entities
{
    /// <inheritdoc/>
    [Index(nameof(LastName), nameof(FirstName), nameof(Patronymic), IsUnique = true, Name = "NameIndex")]
    public abstract class Person : Person<int>
    {

    }

    /// <summary> Сущность описывающая личность </summary>
    /// <typeparam name="TKey">тип ключа сущности</typeparam>
    [Index(nameof(LastName), nameof(FirstName), nameof(Patronymic), IsUnique = true, Name = "NameIndex")]
    public abstract class Person<TKey> : DescriptedEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>Фамилия</summary>
        public string LastName { get; set; }

        /// <summary>Имя</summary>
        public string FirstName { get; set; }

        /// <summary>Отчество</summary>
        public string Patronymic { get; set; }

        /// <inheritdoc/>
        protected Person() { }

        /// <inheritdoc/>
        protected Person(string LastName, string FirstName, string Patronymic)
        {
            this.LastName = LastName;
            this.FirstName = FirstName;
            this.Patronymic = Patronymic;
        }

        /// <inheritdoc/>
        public override string ToString() => $"[id:{Id}] {string.Join(' ', LastName, FirstName, Patronymic)}";
    }
}
