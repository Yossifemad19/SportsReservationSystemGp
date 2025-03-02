using System.ComponentModel;
using System.Linq.Expressions;
using backend.Core.Entities;

namespace backend.Core.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    void Add(T entity);

    
    Task<T> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> FindAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> include);

    Task<T?> GetByIdAsync(int id);

    void Update(T entity);
    void Remove(T entity);
    
    

}