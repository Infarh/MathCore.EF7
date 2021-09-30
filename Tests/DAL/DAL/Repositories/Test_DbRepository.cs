using DAL.Context;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace DAL.Repositories
{
    public class Test_DbRepository<TEntity> : DbRepository<TestContext, TEntity> where TEntity : class, IEntity, new()
    {
        public Test_DbRepository(TestContext db, ILogger<DbRepository<TestContext, TEntity>> Logger) : base(db, Logger)
        {
        }
    }
}
