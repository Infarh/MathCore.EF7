using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathCore.EF7.Extensions;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Repositories.Factory
{
    /// <inheritdoc/>
    public class DbContextFactoryGPSRepository<TContext, TGpsEntity> : DbContextFactoryGPSRepository<TContext, TGpsEntity, int>, IGPSRepository<TGpsEntity>
        where TGpsEntity : class, IGPSEntity, new() where TContext : DbContext
    {
        /// <inheritdoc/>
        public DbContextFactoryGPSRepository(
            IDbContextFactory<TContext> ContextFactory,
            ILogger<DbContextFactoryGPSRepository<TContext, TGpsEntity, int>> Logger) : base(
            ContextFactory,
            Logger)
        {
        }
    }

    /// <inheritdoc cref="DbContextFactoryRepository{TContext, TGpsEntity, TKey}" />
    public class DbContextFactoryGPSRepository<TContext, TGpsEntity, TKey>
        : DbContextFactoryRepository<TContext, TGpsEntity, TKey>,
          IGPSRepository<TGpsEntity, TKey>
        where TGpsEntity : class, IGPSEntity<TKey>, new()
        where TContext : DbContext
    {
        /// <inheritdoc />
        public DbContextFactoryGPSRepository(IDbContextFactory<TContext> db, ILogger<DbContextFactoryGPSRepository<TContext, TGpsEntity, TKey>> Logger) : base(db, Logger) { }

        /// <inheritdoc />
        public async Task<bool> ExistInLocation(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await db.Set<TGpsEntity>()
               .OrderByDistanceInRange<TGpsEntity, TKey>(Latitude, Longitude, RangeInMeters)
               .AnyAsync(Cancel)
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<int> GetCountInLocation(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await db.Set<TGpsEntity>()
               .OrderByDistanceInRange<TGpsEntity, TKey>(Latitude, Longitude, RangeInMeters)
               .CountAsync(Cancel)
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TGpsEntity>> GetAllByLocationInRange(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db).OrderByDistanceInRange<TGpsEntity, TKey>(Latitude, Longitude, RangeInMeters)
               .ToArrayAsync(Cancel)
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TGpsEntity>> GetAllByLocationInRange(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            int Skip,
            int Take,
            CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db)
               .OrderByDistanceInRange<TGpsEntity, TKey>(Latitude, Longitude, RangeInMeters)
               .Skip(Skip)
               .Take(Take)
               .ToArrayAsync(Cancel)
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TGpsEntity> GetByLocation(
            double Latitude,
            double Longitude,
            CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db)
               .OrderByDistance<TGpsEntity, TKey>(Latitude, Longitude)
               .FirstAsync(Cancel)
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TGpsEntity> GetByLocationInRange(
            double Latitude,
            double Longitude,
            double RangeInMeters,
            CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db)
               .OrderByDistance<TGpsEntity, TKey>(Latitude, Longitude)
               .FirstOrDefaultAsync(Cancel)
               .ConfigureAwait(false);
        }

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

            await using var db = ContextFactory.CreateDbContext();
            var query = GetDbQuery(db).OrderByDistanceInRange<TGpsEntity, TKey>(Latitude, Longitude, RangeInMeters);
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
