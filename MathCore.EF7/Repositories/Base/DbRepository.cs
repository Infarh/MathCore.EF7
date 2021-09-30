using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathCore.EF7.Extensions;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Repositories.Base
{
    /// <inheritdoc />
    public class DbRepository<TContext, TEntity> : DbRepository<TContext, TEntity, int> where TEntity : class, IEntity, new() where TContext : DbContext
    {
        /// <inheritdoc />
        protected DbRepository(TContext db, ILogger<DbRepository<TContext, TEntity, int>> Logger) : base(db, Logger)
        {
        }
    }

    /// <summary>Репозиторий сущностей, работающий с контекстом БД</summary>
    /// <typeparam name="TEntity">Тип контролируемых сущностей</typeparam>
    /// <typeparam name="TContext">Тип контекста базы данных</typeparam>
    /// <typeparam name="TKey">тип ключа сущности</typeparam>
    public class DbRepository<TContext, TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new() where TContext : DbContext
    {
        private readonly TContext _db;
        /// <summary> Логгер </summary>
        protected readonly ILogger<DbRepository<TContext, TEntity, TKey>> _Logger;
        /// <summary> DbSet сущности </summary>
        protected DbSet<TEntity> Set { get; }
        /// <summary> все элементы DbSet с возможность настройки (фильтрация выборка и прочее) </summary>
        protected virtual IQueryable<TEntity> Items => Set;
        /// <summary> Флаг необходимости сохранять изменения в базе данных после каждого запроса </summary>
        public bool AutoSaveChanges { get; set; } = true;

        /// <summary> конструктор </summary>
        /// <param name="db">контекст базы данных</param>
        /// <param name="Logger">логгер</param>
        protected DbRepository(TContext db, ILogger<DbRepository<TContext, TEntity, TKey>> Logger)
        {
            _db = db;
            Set = db.Set<TEntity>();
            _Logger = Logger;
        }

        /// <inheritdoc />
        public Task<bool> IsEmpty(CancellationToken Cancel = default) => Set.AnyAsync(Cancel);

        /// <inheritdoc />
        public async Task<bool> ExistId(TKey Id, CancellationToken Cancel = default) =>
            await Set.AnyAsync(EntityExtension.GetId<TEntity, TKey>(Id), cancellationToken: Cancel).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<bool> Exist(TEntity item, CancellationToken Cancel = default)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            return await Set.AnyAsync(EntityExtension.GetId<TEntity,TKey>(item.Id), Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<int> GetCount(CancellationToken Cancel = default) => await Items.CountAsync(Cancel).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetAll(CancellationToken Cancel = default) => await Items.ToArrayAsync(Cancel).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> Get(int Skip, int Count, CancellationToken Cancel = default)
        {
            if (Count <= 0) return Enumerable.Empty<TEntity>();

            var query = Items;
            if (Skip > 0) query = query.Skip(Skip);

            return await query.Take(Count).ToArrayAsync(Cancel);
        }

        /// <inheritdoc />
        public async Task<IPage<TEntity>> GetPage(int PageNumber, int PageSize, CancellationToken Cancel = default)
        {
            if (PageSize <= 0) return new Page<TEntity>(Enumerable.Empty<TEntity>(), PageSize, PageNumber, PageSize);

            var query = Items;
            var total_count = await query.CountAsync(Cancel).ConfigureAwait(false);
            if (total_count == 0) return new Page<TEntity>(Enumerable.Empty<TEntity>(), PageSize, PageNumber, PageSize);

            if (PageNumber > 0) query = query.Skip(PageNumber * PageSize);
            query = query.Take(PageSize);
            var items = await query.ToArrayAsync(Cancel).ConfigureAwait(false);

            return new Page<TEntity>(items, total_count, PageNumber, PageSize);
        }

        /// <inheritdoc />
        public async Task<TEntity> GetById(TKey Id, CancellationToken Cancel = default) => Items switch
        {
            DbSet<TEntity> set => await set.FindAsync(new object[] { Id }, Cancel).ConfigureAwait(false),
            { } items => await items.FirstOrDefaultAsync(EntityExtension.GetId<TEntity,TKey>(Id), Cancel).ConfigureAwait(false),
        };

        /// <inheritdoc />
        public async Task<TEntity> Add(TEntity item, CancellationToken Cancel = default)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            _Logger.LogInformation("Добавление {0} в репозиторий...", item);

            _db.Entry(item).State = EntityState.Added;
            if (AutoSaveChanges) await SaveChanges(Cancel).ConfigureAwait(false);

            _Logger.LogInformation("Добавление {0} в репозиторий выполнено с id: {1}", item, item.Id);

            return item;
        }

        /// <inheritdoc />
        public async Task AddRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));
            _Logger.LogInformation("Добавление множества записей в репозиторий...");
            await _db.AddRangeAsync(items, Cancel).ConfigureAwait(false);
            if (AutoSaveChanges)
            {
                var count = await SaveChanges(Cancel).ConfigureAwait(false);
                _Logger.LogInformation($"Добавление {count} записей в репозиторий выполнено");
            }
            else
                _Logger.LogInformation("Добавление множества записей в репозиторий выполнено и ждет сохранения");

        }

        /// <inheritdoc />
        public async Task<TEntity> Update(TEntity item, CancellationToken Cancel = default)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            _Logger.LogInformation("Обновление id: {0} - {1}...", item.Id, item);


            _db.Update(item);
            if (AutoSaveChanges)
            {
                var count = await SaveChanges(Cancel).ConfigureAwait(false);
                _Logger.LogInformation($"Обновление id: {item.Id} - {item} выполнено, внесено изменений {count}");
            }
            else
                _Logger.LogInformation("Обновление id: {0} - {1} выполнено, ожидает сохранения", item.Id, item);

            return item;
        }

        /// <inheritdoc />
        public async Task<TEntity> UpdateById(TKey id, Action<TEntity> ItemUpdated, CancellationToken Cancel = default)
        {
            _Logger.LogInformation("Обновление id: {0}...", id);
            if (await GetById(id, Cancel).ConfigureAwait(false) is not { } item)
            {
                _Logger.LogInformation("Элемент с id: {0} не найден", id);
                return default;
            }
            ItemUpdated(item);
            if (AutoSaveChanges)
            {
                var count = await SaveChanges(Cancel).ConfigureAwait(false);
                _Logger.LogInformation($"Обновление завершено, внесено изменений {count}");
            }
            else
                _Logger.LogInformation($"Обновление завершено, ожидает сохранения");

            return item;
        }

        /// <inheritdoc />
        public async Task UpdateRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));
            _Logger.LogInformation("Изменение множества записей в репозиторий...");
            _db.UpdateRange(items);
            if (AutoSaveChanges)
            {
                var count = await SaveChanges(Cancel).ConfigureAwait(false);
                _Logger.LogInformation($"Изменение записей произведено, внесено изменений - {count}");
            }
            else
                _Logger.LogInformation($"Изменение записей произведено, ожидает сохранения");

        }

        /// <inheritdoc />
        public async Task<TEntity> Delete(TEntity item, CancellationToken Cancel = default)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            _Logger.LogInformation("Удаление id: {0} - {1}...", item.Id, item);

            _db.Remove(item);
            if (AutoSaveChanges)
            {
                var count = await SaveChanges(Cancel).ConfigureAwait(false);
                _Logger.LogInformation($"Удаление id: {item.Id} - {item} выполнено внесено изменений - {count}");
            }
            else
                _Logger.LogInformation("Удаление id: {0} - {1} выполнено", item.Id, item);

            return item;
        }

        /// <inheritdoc />
        public async Task DeleteRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));
            _db.RemoveRange(items);
            if (AutoSaveChanges)
            {
                var count = await SaveChanges(Cancel).ConfigureAwait(false);
                _Logger.LogInformation($"Удаление записей произведено, внесено изменений - {count}");
            }
            else
                _Logger.LogInformation($"Удаление записей произведено, ожидает сохранения");
        }

        /// <inheritdoc />
        public async Task<TEntity> DeleteById(TKey id, CancellationToken Cancel = default)
        {
            var item = await Set.FindAsync(new object[] { id }, Cancel).ConfigureAwait(false);
            //var item = Set.Local.FirstOrDefault(i => i.Id == id) 
            //    ?? await Set
            //       //.Select(i => new T { Id = i.Id})
            //       .FirstOrDefaultAsync(i => i.Id == id, Cancel)
            //       .ConfigureAwait(false);
            if (item is not null)
                return await Delete(item, Cancel).ConfigureAwait(false);

            _Logger.LogInformation("При удалении записи с id: {0} - запись не найдена", id);
            return null;
        }

        /// <inheritdoc />
        public virtual async Task<int> SaveChanges(CancellationToken Cancel = default)
        {
            _Logger.LogInformation("Сохранение изменений в БД");
            var timer = Stopwatch.StartNew();

            var changes_count = await _db.SaveChangesAsync(Cancel).ConfigureAwait(false);

            timer.Stop();
            _Logger.LogInformation("Сохранение изменений в БД  завершено за {0} c. Изменений {1}", timer.Elapsed.TotalSeconds, changes_count);
            return changes_count;
        }
    }
}
