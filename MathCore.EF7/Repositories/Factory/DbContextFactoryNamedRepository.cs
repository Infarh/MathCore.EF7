using System.Threading;
using System.Threading.Tasks;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Repositories.Factory
{
    /// <inheritdoc cref="DbContextFactoryRepository{TDb,T}" />
    public class DbContextFactoryNamedRepository<TDb, T> : DbContextFactoryRepository<TDb, T>, INamedRepository<T> where T : class, INamedEntity, new() where TDb:DbContext
    {
        /// <inheritdoc />
        public DbContextFactoryNamedRepository(IDbContextFactory<TDb> ContextFactory, ILogger<DbContextFactoryNamedRepository<TDb, T>> Logger) : base(ContextFactory, Logger) { }

        /// <inheritdoc />
        public async Task<bool> ExistName(string Name, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db).AnyAsync(item => item.Name == Name, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<T> GetByName(string Name, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db).FirstOrDefaultAsync(item => item.Name == Name, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<T> DeleteByName(string Name, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            var item = await db.Set<T>()
               //.Select(i => new T { Id = i.Id, Name = i.Name })
               .FirstOrDefaultAsync(i => i.Name == Name, Cancel)
               .ConfigureAwait(false);
            if (item is not null) return await Delete(item, Cancel).ConfigureAwait(false);

            _Logger.LogInformation("При удалении записи с Name: {0} - запись не найдена", Name);
            return null;
        }
    }
}
