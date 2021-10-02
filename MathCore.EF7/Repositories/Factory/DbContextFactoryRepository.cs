using System;
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
    /// <inheritdoc cref="DbContextFactoryRepository{TContext, TEntity, TKey}" />
    public class DbContextFactoryRepository<TContext, TEntity> : DbContextFactoryRepository<TContext, TEntity, int>, IRepository<TEntity>
        where TEntity : class, IEntity, new()
        where TContext : DbContext
    {
        /// <inheritdoc />
        public DbContextFactoryRepository(IDbContextFactory<TContext> ContextFactory, ILogger<DbContextFactoryRepository<TContext, TEntity, int>> Logger) : base(ContextFactory, Logger)
        {
        }
    }

    /// <summary> Фабрика контекста репозиториев сущностей </summary>
    /// <typeparam name="TContext">тип контекста базы данных</typeparam>
    /// <typeparam name="TEntity">тип сущности</typeparam>
    /// <typeparam name="TKey">тип ключа сущностей</typeparam>
    public class DbContextFactoryRepository<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
        where TContext : DbContext
    {
        /// <summary> Контекст фабрики </summary>
        protected IDbContextFactory<TContext> ContextFactory { get; }
        /// <summary> логгер </summary>
        protected readonly ILogger<DbContextFactoryRepository<TContext, TEntity, TKey>> _Logger;
        /// <summary> Отслеживать выдаваемые объекты в контексте БД </summary>
        public bool NoTracked { get; set; } = true;

        /// <summary> Конструктор </summary>
        /// <param name="ContextFactory">контекст</param>
        /// <param name="Logger">логгер</param>
        public DbContextFactoryRepository(IDbContextFactory<TContext> ContextFactory, ILogger<DbContextFactoryRepository<TContext, TEntity, TKey>> Logger)
        {
            this.ContextFactory = ContextFactory;
            _Logger = Logger;
        }
        /// <summary> пучить DBSet </summary>
        /// <param name="db">контекст базы данных</param>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> GetDbQuery(DbContext db) => NoTracked
            ? db.Set<TEntity>().AsNoTracking()
            : db.Set<TEntity>();

        /// <summary> Упорядоченные сущности </summary>
        protected IQueryable<TEntity> GetOrderedDbQuery(DbContext db) =>
            GetDbQuery(db) switch
            {
                IOrderedQueryable<TEntity> ordereq_query => ordereq_query,
                { } q => q.OrderBy(i => i.Id)
            };

        /// <inheritdoc />
        public virtual async Task<bool> IsEmpty(CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await db.Set<TEntity>().AnyAsync(Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<bool> ExistId(TKey Id, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db).AnyAsync(EntityExtension.GetId<TEntity,TKey>(Id), Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<bool> Exist(TEntity item, CancellationToken Cancel = default)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db).AnyAsync(EntityExtension.GetId<TEntity, TKey>(item.Id), Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<int> GetCount(CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db).CountAsync(Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAll(CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db).ToArrayAsync(Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> Get(int Skip, int Count, CancellationToken Cancel = default)
        {
            if (Count <= 0) return Enumerable.Empty<TEntity>();

            await using var db = ContextFactory.CreateDbContext();
            var query = Skip <= 0 ? GetOrderedDbQuery(db) : GetOrderedDbQuery(db).Skip(Skip);
            return await query.Take(Count).ToArrayAsync(Cancel);
        }

        /// <inheritdoc />
        public virtual async Task<IPage<TEntity>> GetPage(int PageNumber, int PageSize, CancellationToken Cancel = default)
        {
            if (PageSize <= 0) return new Page<TEntity>(Enumerable.Empty<TEntity>(), PageSize, PageNumber, PageSize);

            await using var db = ContextFactory.CreateDbContext();

            IQueryable<TEntity> query = GetOrderedDbQuery(db);
            var total_count = await query.CountAsync(Cancel).ConfigureAwait(false);
            if (total_count == 0) return new Page<TEntity>(Enumerable.Empty<TEntity>(), PageSize, PageNumber, PageSize);

            if (PageNumber > 0) query = query.Skip(PageNumber * PageSize);
            query = query.Take(PageSize);
            var items = await query.ToArrayAsync(Cancel).ConfigureAwait(false);

            return new Page<TEntity>(items, total_count, PageNumber, PageSize);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> GetById(TKey Id, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return GetDbQuery(db) switch
            {
                DbSet<TEntity> set => await set.FindAsync(new object[] { Id }, Cancel).ConfigureAwait(false),
                { } query => query.FirstOrDefault(EntityExtension.GetId<TEntity, TKey>(Id)),
                _ => throw new InvalidOperationException()
            };
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Add(TEntity item, CancellationToken Cancel = default)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            await using var db = ContextFactory.CreateDbContext();
            _Logger.LogInformation("Добавление {0} в репозиторий...", item);

            db.Entry(item).State = EntityState.Added;
            await db.SaveChangesAsync(Cancel).ConfigureAwait(false);

            _Logger.LogInformation("Добавление {0} в репозиторий выполнено с id: {1}", item, item.Id);

            return item;
        }

        /// <inheritdoc />
        public virtual async Task AddRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            await db.AddRangeAsync(items, Cancel).ConfigureAwait(false);
            await db.SaveChangesAsync(Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Update(TEntity item, CancellationToken Cancel = default)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            await using var db = ContextFactory.CreateDbContext();
            await Update(db, item, Cancel).ConfigureAwait(false);

            _Logger.LogInformation("Обновление id: {0} - {1} выполнено", item.Id, item);
            return item;
        }

        private static Task Update(TContext db, TEntity item, CancellationToken Cancell)
        {
            db.Entry(item).State = EntityState.Modified;
            return db.SaveChangesAsync(Cancell);
        }
        /// <inheritdoc />
        public virtual async Task<TEntity> UpdateById(TKey id, Action<TEntity> ItemUpdated, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();

            if (await GetById(id, Cancel).ConfigureAwait(false) is not { } item)
                return default;
            ItemUpdated(item);
            await Update(db, item, Cancel).ConfigureAwait(false);
            return item;
        }

        /// <inheritdoc />
        public virtual async Task UpdateRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            db.UpdateRange(items);
            await db.SaveChangesAsync(Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Delete(TEntity item, CancellationToken Cancel = default)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            await using var db = ContextFactory.CreateDbContext();

            db.Entry(item).State = EntityState.Deleted;
            await db.SaveChangesAsync(Cancel).ConfigureAwait(false);

            _Logger.LogInformation("Удаление id: {0} - {1} выполнено", item.Id, item);
            return item;
        }

        /// <inheritdoc />
        public virtual async Task DeleteRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            db.RemoveRange(items);
            await db.SaveChangesAsync(Cancel).ConfigureAwait(false);
        }


        /// <inheritdoc />
        public virtual async Task<TEntity> DeleteById(TKey id, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            var item = await db.Set<TEntity>().FindAsync(new object[] { id }, Cancel).ConfigureAwait(false);
            //var item = await db.Set<T>()
            //       .Select(i => new T { Id = i.Id })
            //       .FirstOrDefaultAsync(i => i.Id == id, Cancel)
            //       .ConfigureAwait(false);
            if (item is not null) return await Delete(item, Cancel).ConfigureAwait(false);

            _Logger.LogInformation("При удалении записи с id: {0} - запись не найдена", id);
            return null;

        }

        /// <inheritdoc />
        public virtual Task<int> SaveChanges(CancellationToken Cancel = default)
            => throw new NotImplementedException("Недоступно для реализации в контексте фабрики");

    }
}
