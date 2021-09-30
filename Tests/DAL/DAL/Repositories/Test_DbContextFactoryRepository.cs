using DAL.Context;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Repositories.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Repositories
{
    public class Test_DbContextFactoryRepository<TEntity> : DbContextFactoryRepository<TestContext, TEntity> where TEntity : class, IEntity, new()
    {
        public Test_DbContextFactoryRepository(
            IDbContextFactory<TestContext> ContextFactory,
            ILogger<DbContextFactoryRepository<TestContext, TEntity>> Logger) : base(
            ContextFactory,
            Logger)
        {
        }
    }
}