using System.Linq.Expressions;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Data;

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
        var entity= await _dbSet.FirstOrDefaultAsync(predicate);
        return entity;
    }
    
    // for test login
    public async Task<Entity> FindAsync(Expression<Func<Entity, bool>> predicate,Expression<Func<Entity,object>> include)
    {
        var entity= await _dbSet.Include(include).FirstOrDefaultAsync(predicate);
        return entity;
    }
}