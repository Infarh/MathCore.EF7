using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathCore.EF7.Extensions;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;
using MathCore.EF7.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Repositories
{
    /// <inheritdoc />
    public class DbGPSRepository<TContext, TGpsEntity> : DbGPSRepository<TContext, TGpsEntity, int>
        where TGpsEntity : class, IGPSEntity, new() where TContext : DbContext
    {
        /// <inheritdoc />
        public DbGPSRepository(TContext db, ILogger<DbGPSRepository<TContext, TGpsEntity, int>> Logger) : base(db, Logger)
        {
        }
    }

    /// <summary>Репозиторий географических данных</summary>
    /// <typeparam name="TGpsEntity">Тип сущности репозитория</typeparam>
    /// <typeparam name="TContext">Тип контекста базы данных</typeparam>
    /// <typeparam name="TKey">Тип ключа сущности</typeparam>
    public class DbGPSRepository<TContext, TGpsEntity, TKey>
        : DbRepository<TContext, TGpsEntity, TKey>,
          IGPSRepository<TGpsEntity, TKey>
        where TGpsEntity : class, IGPSEntity<TKey>, new()
        where TContext : DbContext
    {
        /// <summary>Инициализация нового экземпляра <see cref="DbGPSRepository{TContext, TGpsEntity, TKey}"/></summary>
        /// <param name="db">Контекст БД</param>
        /// <param name="Logger">Логгер</param>
        public DbGPSRepository(TContext db, ILogger<DbGPSRepository<TContext, TGpsEntity, TKey>> Logger) : base(db, Logger) { }

        /// <inheritdoc />
        public Task<bool> ExistInLocation(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            CancellationToken Cancel = default) =>
            Set
               .OrderByDistanceInRange<TGpsEntity, TKey>(Latitude, Longitude, RangeInMeters)
               .AnyAsync(Cancel);

        /// <inheritdoc />
        public Task<int> GetCountInLocation(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            CancellationToken Cancel = default) =>
            Set
               .OrderByDistanceInRange<TGpsEntity, TKey>(Latitude, Longitude, RangeInMeters)
               .CountAsync(Cancel);

        /// <inheritdoc />
        public async Task<IEnumerable<TGpsEntity>> GetAllByLocationInRange(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            CancellationToken Cancel = default) =>
            await Items
               .OrderByDistanceInRange<TGpsEntity, TKey>(Latitude, Longitude, RangeInMeters)
               .ToArrayAsync(Cancel)
               .ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<IEnumerable<TGpsEntity>> GetAllByLocationInRange(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            int Skip,
            int Take,
            CancellationToken Cancel = default) =>
            await Items
               .OrderByDistanceInRange<TGpsEntity, TKey>(Latitude, Longitude, RangeInMeters)
               .Skip(Skip)
               .Take(Take)
               .ToArrayAsync(Cancel)
               .ConfigureAwait(false);

        /// <inheritdoc />
        public Task<TGpsEntity> GetByLocation(
            double Latitude,
            double Longitude,
            CancellationToken Cancel = default) =>
            Items
               .OrderByDistance<TGpsEntity, TKey>(Latitude, Longitude)
               .FirstAsync(Cancel);

        /// <inheritdoc />
        public Task<TGpsEntity> GetByLocationInRange(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            CancellationToken Cancel = default) =>
            Items.OrderByDistanceInRange<TGpsEntity, TKey>(Latitude, Longitude, RangeInMeters)
               .FirstOrDefaultAsync(Cancel);

        /// <inheritdoc />
        public async Task<IPage<TGpsEntity>> GetPageByLocationInRange(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            int PageNumber,
            int PageSize,
            CancellationToken Cancel = default)
        {
            if (PageSize <= 0) return new Page<TGpsEntity>(Enumerable.Empty<TGpsEntity>(), PageSize, PageNumber, PageSize);

            var query = Items.OrderByDistanceInRange<TGpsEntity, TKey>(Latitude, Longitude, RangeInMeters);
            var total_count = await query.CountAsync(Cancel).ConfigureAwait(false);
            if (total_count == 0) return new Page<TGpsEntity>(Enumerable.Empty<TGpsEntity>(), PageSize, PageNumber, PageSize);

            if (PageNumber > 0) query = query.Skip(PageNumber * PageSize);
            query = query.Take(PageSize);
            var items = await query.ToArrayAsync(Cancel).ConfigureAwait(false);

            return new Page<TGpsEntity>(items, total_count, PageNumber, PageSize);
        }

        /// <inheritdoc />
        public async Task<TGpsEntity> DeleteByLocation(double Latitude, double Longitude, CancellationToken Cancel = default) =>
            await GetByLocation(Latitude, Longitude, Cancel).ConfigureAwait(false) is { } item
                ? await Delete(item, Cancel)
                : default;

        /// <inheritdoc />
        public async Task<TGpsEntity> DeleteByLocationInRange(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            CancellationToken Cancel = default) =>
            await GetByLocationInRange(Latitude, Longitude, RangeInMeters, Cancel).ConfigureAwait(false) is { } item
                ? await Delete(item, Cancel)
                : default;
    }
}
