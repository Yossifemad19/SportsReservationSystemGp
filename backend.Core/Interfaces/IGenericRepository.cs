using System.Linq.Expressions;
using backend.Core.Entities;

namespace backend.Core.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<int> AddAsync(T entity);
    Task<T> FindAsync(Expression<Func<T, bool>> predicate);
}