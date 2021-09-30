using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;
using MathCore.EF7.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Repositories
{
    /// <inheritdoc cref="DbNamedRepository{TContext,TKey}" />
    public class DbNamedRepository<TContext, TNamedEntity> : DbNamedRepository<TContext, TNamedEntity, int>
        where TNamedEntity : class, INamedEntity, new()
        where TContext : DbContext
    {
        /// <inheritdoc />
        public DbNamedRepository(TContext db, ILogger<DbNamedRepository<TContext, TNamedEntity, int>> Logger) : base(db, Logger)
        {
        }
    }

    /// <inheritdoc cref="DbRepository{TContext, TNamedEntity,TKey}" />
    public class DbNamedRepository<TContext, TNamedEntity, TKey>
        : DbRepository<TContext, TNamedEntity, TKey>,
          INamedRepository<TNamedEntity, TKey>
        where TNamedEntity : class, INamedEntity<TKey>, new()
        where TContext : DbContext
    {
        /// <inheritdoc />
        public DbNamedRepository(TContext db, ILogger<DbNamedRepository<TContext, TNamedEntity, TKey>> Logger) : base(db, Logger) { }

        /// <inheritdoc />
        public async Task<bool> ExistName(string Name, CancellationToken Cancel = default) =>
            await Set.AnyAsync(item => item.Name == Name, Cancel).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<TNamedEntity> GetByName(string Name, CancellationToken Cancel = default) =>
            await Items.FirstOrDefaultAsync(item => item.Name == Name, Cancel).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<TNamedEntity> DeleteByName(string Name, CancellationToken Cancel = default)
        {
            var item = Set.Local.FirstOrDefault(i => i.Name == Name)
                ?? await Set
                   //.Select(i => new T { Id = i.Id, Name = i.Name })
                   .FirstOrDefaultAsync(i => i.Name == Name, Cancel)
                   .ConfigureAwait(false);
            if (item is not null) return await Delete(item, Cancel).ConfigureAwait(false);

            _Logger.LogInformation("При удалении записи с Name: {0} - запись не найдена", Name);
            return null;
        }
    }
}
