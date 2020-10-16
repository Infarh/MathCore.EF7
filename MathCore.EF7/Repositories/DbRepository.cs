using System;
using System.Linq;
using System.Threading.Tasks;

using MathCore.EF7.Repositories.Base;

using Microsoft.EntityFrameworkCore;

namespace MathCore.EF7.Repositories
{
    public abstract class DbRepository<T> : IDbRepository<T>
        where T : class, IEntity
    {
        private readonly DbContext _Context;
        protected readonly DbSet<T> _Set;

        public bool AutoSaveChanges { get; set; } = true;

        protected DbRepository(DbContext Context)
        {
            _Context = Context ?? throw new ArgumentNullException(nameof(Context));
            _Set = Context.Set<T>();
        }

        #region IDbRepository<T>

        #region IRepository<T>

        public virtual IQueryable<T> Items => _Set;

        public virtual async Task<T> Get(int id)
        {
            var items = Items;
            return items switch
            {
                null => throw new InvalidOperationException("Отсутствует ссылка на множество данных"),
                DbSet<T> set => await set.FindAsync(id).ConfigureAwait(false),
                _ => await items.SingleOrDefaultAsync(item => item.Id == id).ConfigureAwait(false)
            };
        }

        public virtual async Task<T> Add(T Item)
        {
            var item = (await _Set.AddAsync(Item).ConfigureAwait(false)).Entity;
            if (AutoSaveChanges)
                await _Context.SaveChangesAsync().ConfigureAwait(false);
            return item;
        }

        public virtual async Task<T> Update(T Item)
        {
            _Context.Entry(Item).State = EntityState.Modified;
            if (AutoSaveChanges)
                await SaveChanges().ConfigureAwait(false);
            return Item;
        }

        public virtual async Task<T> Delete(int id)
        {
            var item = await Get(id).ConfigureAwait(false);
            if (item is null) return null;
            _Context.Entry(item).State = EntityState.Deleted;
            if (AutoSaveChanges)
                await SaveChanges().ConfigureAwait(false);
            return item;
        }

        #endregion

        public virtual async Task SaveChanges() => await _Context.SaveChangesAsync().ConfigureAwait(false);

        #endregion
    }
}
