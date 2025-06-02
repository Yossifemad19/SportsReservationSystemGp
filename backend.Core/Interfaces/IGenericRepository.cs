using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.Core.Specification;

namespace backend.Core.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);

    Task<T?> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    Task<T> FindAsync(Expression<Func<T, bool>> predicate);
    // Task<T> FindAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> include);
    Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties);
    Task<T> GetByIdAsync(int id);
    Task<T> GetByIdAsync(string id);

    Task<ICollection<T>> GetAllAsync();
    Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec);
    Task<T> GetByIdWithSpecAsync(ISpecification<T> spec);
    Task<T> GetFirstOrDefaultAsync(ISpecification<T> spec);




}