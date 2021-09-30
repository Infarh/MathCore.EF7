using System;
using System.Linq.Expressions;
using MathCore.EF7.Interfaces.Entities;

namespace MathCore.EF7.Extensions
{
    /// <summary>
    /// Расширение для сравнения сущностей
    /// </summary>
    public static class EntityExtension
    {
        /// <summary> сравнение сущностей по id </summary>
        /// <param name="id">id</param>
        /// <typeparam name="TEntity">Тип сущности</typeparam>
        /// <typeparam name="TKey">тип ключа для сравнения</typeparam>
        /// <returns></returns>
        public static Expression<Func<TEntity, bool>> GetId<TEntity, TKey>(TKey id) where TEntity : class, IEntity<TKey>
        {
            var entity = Expression.Parameter(typeof(TEntity), "entity");
            var id_property = Expression.Property(entity, nameof(IEntity<TKey>.Id));
            var equals = Expression.Equal(id_property, Expression.Constant(id));
            var expression = Expression.Lambda<Func<TEntity, bool>>(equals, entity);
            return expression;
        }
    }
}