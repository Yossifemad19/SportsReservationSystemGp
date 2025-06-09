using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;
using backend.Repository.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data;

public class GenericRepository<Entity> : IGenericRepository<Entity> where Entity : BaseEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<Entity> _dbSet;
    
    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<Entity>();
    }
    public void Add(Entity entity)
    {
        _dbSet.Add(entity);
    }

    public async Task<Entity> FindAsync(Expression<Func<Entity, bool>> predicate)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(predicate);
        return entity;
    }
    
    public void Update(Entity entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Remove(Entity entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<Entity> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<Entity> GetByIdAsync(string id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<ICollection<Entity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<Entity>> GetAllIncludingAsync(params Expression<Func<Entity, object>>[] includeProperties)
    {
        IQueryable<Entity> query = _dbSet;
        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Entity>> GetAllIncludingAsync(Expression<Func<Entity, bool>> predicate,params Expression<Func<Entity, object>>[] includeProperties)
    {
        IQueryable<Entity> query = _dbSet;

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        query = query.Where(predicate);

        return await query.ToListAsync();
    }

    public IQueryable<Entity> Query()
    {
        return _dbSet.AsQueryable();
    }

    public async Task<Entity?> FindAsync(Expression<Func<Entity, bool>> predicate,params Expression<Func<Entity, object>>[] includes)
    {
        IQueryable<Entity> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(predicate);
    }


    public async Task<IReadOnlyList<Entity>> GetAllWithSpecAsync(ISpecification<Entity> spec)
    {
        var query = ApplySpecification(spec);
        return await query.ToListAsync();
    }

    public async Task<Entity> GetByIdWithSpecAsync(ISpecification<Entity> spec)
    {
        var query = ApplySpecification(spec);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<Entity> GetFirstOrDefaultAsync(ISpecification<Entity> spec)
    {
        var query = ApplySpecification(spec);
        return await query.FirstOrDefaultAsync();
    }

    private IQueryable<Entity> ApplySpecification(ISpecification<Entity> spec)
    {
        return SpecificationEvaluator<Entity>.GetQuery(_dbSet.AsQueryable(), spec);
    }
    // // for test login
    // public async Task<Entity> FindAsync(Expression<Func<Entity, bool>> predicate,Expression<Func<Entity,object>> include)
    // {
    //     var entity= await _dbSet.Include(include).FirstOrDefaultAsync(predicate);
    //     return entity;
    // }

}

