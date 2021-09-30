using DAL.Context;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;
using MathCore.EF7.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace DAL.Repositories
{
    public class Test_DbRepository<TEntity> : Test_DbRepository<TEntity, int>, IRepository<TEntity> where TEntity : class, IEntity, new()
    {
        public Test_DbRepository(TestContext db, ILogger<Test_DbRepository<TEntity, int>> Logger) : base(db, Logger)
        {
        }
    }
    public class Test_DbRepository<TEntity,Tkey> : DbRepository<TestContext, TEntity, Tkey>, IRepository<TEntity,Tkey> where TEntity : class, IEntity<Tkey>, new()
    {
        protected Test_DbRepository(TestContext db, ILogger<Test_DbRepository<TEntity, Tkey>> Logger) : base(db, Logger)
        {
        }
    }
}
