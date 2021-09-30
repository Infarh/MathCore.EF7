using DAL.Context;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;
using MathCore.EF7.Repositories.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Repositories
{
    public class Test_DbContextFactoryRepository<TEntity> : Test_DbContextFactoryRepository<TEntity, int>, IRepository<TEntity> where TEntity : class, IEntity, new()
    {
        public Test_DbContextFactoryRepository(IDbContextFactory<TestContext> ContextFactory, ILogger<Test_DbContextFactoryRepository<TEntity, int>> Logger) : base(ContextFactory, Logger)
        {
        }
    }
    public class Test_DbContextFactoryRepository<TEntity,Tkey> : DbContextFactoryRepository<TestContext, TEntity, Tkey> where TEntity : class, IEntity<Tkey>, new()
    {
        public Test_DbContextFactoryRepository(
            IDbContextFactory<TestContext> ContextFactory,
            ILogger<Test_DbContextFactoryRepository<TEntity, Tkey>> Logger) : base(
            ContextFactory,
            Logger)
        {
        }
    }
}