using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace CryptoApp.Data.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly CryptoAppDbContext _context;
    private readonly DbSet<T> _entities;

    public Repository(CryptoAppDbContext context)
    {
        _context = context;
        _entities = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _entities.ToListAsync();
    }

    public async Task<T> FindAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        IQueryable<T> query = _entities.Where(predicate);

        if (include != null)
            query = include(query);

        var entity = await query.FirstOrDefaultAsync();

        if (entity == null)
            throw new NullReferenceException($"Entity of type {typeof(T).Name} not found.");

        return entity;
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        return await _entities.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await _entities.AddAsync(entity);
    }

    public void UpdateAsync(T entity)
    {
        _entities.Update(entity);
    }

    public void Remove(T entity)
    {
        _entities.Remove(entity);
    }
}