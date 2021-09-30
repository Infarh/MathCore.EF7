using System;
using System.Collections.Generic;
using MathCore.EF7.Interfaces.Repositories;

namespace MathCore.EF7.Repositories
{
    /// <summary> Реализация интерфейса постраничных данных </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    internal record Page<T>(IEnumerable<T> Items, int TotalCount, int PageNumber, int PageSize) : IPage<T>
    {
        /// <summary>Полное число страниц в выдаче</summary>
        public int TotalPagesCount => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>Существует ли предыдущая страница</summary>
        public bool HasPrevPage => PageNumber > 0;

        /// <summary>Существует ли следующая страница</summary>
        public bool HasNextPage => PageNumber < TotalPagesCount - 1;//отсчёт от 0 страницы

    }
}
