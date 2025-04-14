using System.Linq.Expressions;
using backend.Core.Entities;

namespace backend.Core.Specification;

public interface ISpecification<T> where T:BaseEntity
{
    public Expression<Func<T,bool>> Criteria { get; }
    public List<Expression<Func<T,object>>> Includes { get; }   
}