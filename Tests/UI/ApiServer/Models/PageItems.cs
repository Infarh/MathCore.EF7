using System.Collections.Generic;
using MathCore.EF7.Interfaces.Repositories;

namespace ApiServer.Models
{
    /// <summary> Реализация интерфейса постраничных данных </summary>
    internal record PageItems<TEntity>(IEnumerable<TEntity> Items, int TotalCount, int PageNumber, int PageSize) : IPage<TEntity>;
}
