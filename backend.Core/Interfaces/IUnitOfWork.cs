using backend.Core.Entities;

namespace backend.Core.Interfaces;

public interface IUnitOfWork:IDisposable
{
    IGenericRepository<T> Repository<T>() where T:BaseEntity;
    Task<int> CompleteAsync();
}