using backend.Core.Entities;
using backend.Core.Specification;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Data;

public static class SpecificationEvaluator<T> where T : BaseEntity
{
    public static IQueryable<T> GetQuery(IQueryable<T> queryInput, ISpecification<T> spec)
    {
        var query = queryInput;

        if (spec.Criteria is not null)
        {
            query = query.Where(spec.Criteria);
        }

        // Apply includes
        query = spec.Includes.Aggregate(query, (current, include) 
            => current.Include(include));
        
        // Apply string-based includes
        query = spec.IncludeStrings.Aggregate(query, (current, include) 
            => current.Include(include));

        return query;
    }
}