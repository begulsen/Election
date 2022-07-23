using System.Threading.Tasks;

namespace ElectionApp.Core
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<string> SaveAsync(TEntity entity);
    }
}