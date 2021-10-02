using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;

namespace MathCore.EF7.Extensions
{
    /// <summary> Расширение для репозиториев </summary>
    public static class RepositoryExtensions
    {
        /// <summary>Перечисление всех страниц сущностей репозитория</summary>
        /// <typeparam name="TEntity">Тип сущности репозитория</typeparam>
        /// <typeparam name="TKey">Тип первичного ключа сущности репозитория</typeparam>
        /// <param name="repository">Репозиторий, перечисление страниц которого надо выполнить</param>
        /// <param name="PageSize">Размер страницы</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Последовательное перечисление страниц сущностей репозитория</returns>
        /// <exception cref="ArgumentOutOfRangeException">Если размер страницы меньше, либо равен 0</exception>
        public static async IAsyncEnumerable<IPage<TEntity>> EnumPages<TEntity, TKey>(
            this IRepository<TEntity, TKey> repository,
            int PageSize, 
            [EnumeratorCancellation] CancellationToken Cancel = default) 
            where TEntity : IEntity<TKey>
        {
            if (repository is null) throw new ArgumentNullException(nameof(repository));
            if (PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(PageSize), PageSize, "Размер страницы должен быть больше нуля");

            IPage<TEntity> page;
            var index = 0;
            do
            {
                page = await repository.GetPage(index++, PageSize, Cancel).ConfigureAwait(false);
                yield return page;
            }
            while (page.HasNextPage);
        }
    }
}
