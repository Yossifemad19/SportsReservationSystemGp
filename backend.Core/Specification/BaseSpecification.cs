using System.Linq.Expressions;
using backend.Core.Entities;

namespace backend.Core.Specification;

public class BaseSpecification<T> : ISpecification<T> where T : BaseEntity
{
    public Expression<Func<T, bool>> Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();
    public List<string> IncludeStrings { get; } = new List<string>();

    public BaseSpecification()
    {
    }

    public BaseSpecification(Expression<Func<T, bool>> criteriaExpression)
    {
        Criteria = criteriaExpression;
    }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
    
    // Add this method for string-based includes that support nesting
    protected void AddIncludeString(string includeString)
    {
        IncludeStrings.Add(includeString);
    }
}