using System.Linq;
using System.Threading.Tasks;

namespace MathCore.EF7.Repositories.Base
{
    public interface IRepository<T> where T : class, IEntity
    {
        IQueryable<T> Items { get; }

        Task<T> Get(int id);
        
        Task<T> Add(T Item);
        
        Task<T> Update(T Item);
        
        Task<T> Delete(int id);
    }

    public interface IDbRepository<T> : IRepository<T> where T : class, IEntity
    {
        Task SaveChanges();
    }
}
