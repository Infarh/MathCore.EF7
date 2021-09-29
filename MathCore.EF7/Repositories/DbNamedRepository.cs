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
    /// <inheritdoc cref="DbRepository{TDb,T}" />
    public class DbNamedRepository<TDb,T> : DbRepository<TDb,T>, INamedRepository<T> where T : class, INamedEntity, new() where TDb:DbContext
    {
        /// <inheritdoc />
        public DbNamedRepository(TDb db, ILogger<DbNamedRepository<TDb, T>> Logger) : base(db, Logger) { }

        /// <inheritdoc />
        public async Task<bool> ExistName(string Name, CancellationToken Cancel = default) =>
            await Set.AnyAsync(item => item.Name == Name, Cancel).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<T> GetByName(string Name, CancellationToken Cancel = default) =>
            await Items.FirstOrDefaultAsync(item => item.Name == Name, Cancel).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<T> DeleteByName(string Name, CancellationToken Cancel = default)
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
