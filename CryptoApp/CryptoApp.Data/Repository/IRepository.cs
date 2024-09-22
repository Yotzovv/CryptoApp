using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace CryptoApp.Data.Repository;

public interface IRepository<T> where T : class
{
        Task<IEnumerable<T>> GetAllAsync();
        
        Task<T> FindAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);
        
        Task<T> GetByIdAsync(Guid id);
        
        Task AddAsync(T entity);

        void UpdateAsync(T entity);

        void Remove(T entity);

}