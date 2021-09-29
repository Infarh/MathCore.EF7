using System.Collections.Generic;
using MathCore.EF7.Interfaces.Repositories;

namespace MathCore.EF7.Repositories
{
    /// <summary> Реализация интерфейса постраничных данных </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    internal record Page<T>(IEnumerable<T> Items, int TotalCount, int PageNumber, int PageSize) : IPage<T>;
}
