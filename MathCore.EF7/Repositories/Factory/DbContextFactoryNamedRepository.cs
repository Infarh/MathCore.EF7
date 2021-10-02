using System.Threading;
using System.Threading.Tasks;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Repositories.Factory
{
    /// <inheritdoc />
    public class DbContextFactoryNamedRepository<TDbContext, TNamedEntity> : DbContextFactoryNamedRepository<TDbContext, TNamedEntity, int>
        where TNamedEntity : class, INamedEntity, new() where TDbContext : DbContext
    {
        /// <inheritdoc />
        public DbContextFactoryNamedRepository(IDbContextFactory<TDbContext> ContextFactory, ILogger<DbContextFactoryNamedRepository<TDbContext, TNamedEntity, int>> Logger) : base(ContextFactory, Logger)
        {
        }
    }

    /// <inheritdoc cref="DbContextFactoryRepository{TDbContext, TNamedEntity, TKey}" />
    public class DbContextFactoryNamedRepository<TDbContext, TNamedEntity, TKey>
        : DbContextFactoryRepository<TDbContext, TNamedEntity, TKey>,
          INamedRepository<TNamedEntity, TKey>
        where TNamedEntity : class, INamedEntity<TKey>, new()
        where TDbContext : DbContext
    {
        /// <inheritdoc />
        public DbContextFactoryNamedRepository(IDbContextFactory<TDbContext> ContextFactory, ILogger<DbContextFactoryNamedRepository<TDbContext, TNamedEntity, TKey>> Logger) : base(ContextFactory, Logger) { }

        /// <inheritdoc />
        public async Task<bool> ExistName(string Name, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db).AnyAsync(item => item.Name == Name, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TNamedEntity> GetByName(string Name, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            return await GetDbQuery(db).FirstOrDefaultAsync(item => item.Name == Name, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TNamedEntity> DeleteByName(string Name, CancellationToken Cancel = default)
        {
            await using var db = ContextFactory.CreateDbContext();
            var item = await db.Set<TNamedEntity>()
               //.Select(i => new T { Id = i.Id, Name = i.Name })
               .FirstOrDefaultAsync(i => i.Name == Name, Cancel)
               .ConfigureAwait(false);
            if (item is not null) return await Delete(item, Cancel).ConfigureAwait(false);

            _Logger.LogInformation("При удалении записи с Name: {0} - запись не найдена", Name);
            return null;
        }
    }
}
