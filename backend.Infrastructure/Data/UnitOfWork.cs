using System.Collections;
using backend.Core.Entities;
using backend.Core.Interfaces;

namespace backend.Repository.Data;

public class UnitOfWork : IUnitOfWork
{
    
    private Hashtable _repositories;
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        if(_repositories==null) _repositories = new Hashtable();

        var type =typeof(T).Name;

        if(!_repositories.ContainsKey(type)) { 
            
            var repoType = typeof(GenericRepository<>);

            var repo=Activator.CreateInstance(repoType.MakeGenericType(typeof(T)),_context);

            _repositories.Add(type, repo);
        }

        return (IGenericRepository<T>) _repositories[type];
    }
}