using backend.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : BaseEntity;
    IBookingRepository BookingRepository { get; }
    Task<int> Complete();
    Task<IDbContextTransaction> BeginTransactionAsync();
}