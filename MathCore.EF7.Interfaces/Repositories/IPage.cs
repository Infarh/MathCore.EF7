using System;
using System.Collections.Generic;

namespace MathCore.EF7.Interfaces.Repositories
{
    /// <summary>Страница элементов</summary>
    /// <typeparam name="T">Тип элемента на странице</typeparam>
    public interface IPage<out T>
    {
        /// <summary>Элементы страницы</summary>
        IEnumerable<T> Items { get; }

        /// <summary>Полное количество элементов на всех страницах</summary>
        int TotalCount { get; }

        /// <summary>Номер текущей страницы</summary>
        int PageNumber { get; }

        /// <summary>Размер страницы - число элементов в <see cref="Items"/></summary>
        int PageSize { get; }

        /// <summary>Полное число страниц в выдаче</summary>
        public int TotalPagesCount => PageSize <= 0 ? TotalCount : (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>Существует ли предыдущая страница</summary>
        public bool HasPrevPage => PageNumber > 0;

        /// <summary>Существует ли следующая страница</summary>
        public bool HasNextPage => PageNumber < TotalPagesCount - 1;//отсчёт от 0 страницы
    }
}
