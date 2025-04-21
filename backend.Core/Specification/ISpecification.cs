using System.Linq.Expressions;
using backend.Core.Entities;

namespace backend.Core.Specification;

public interface ISpecification<T> where T : BaseEntity
{
    Expression<Func<T, bool>> Criteria { get;  }
    List<Expression<Func<T, object>>> Includes { get; }
}