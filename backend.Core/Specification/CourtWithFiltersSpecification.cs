using System.Linq.Expressions;
using backend.Core.Entities;
using backend.Core.Specification;
using backend.Core.Specifications;

namespace backend.Core.Specifications;

public class CourtWithFiltersSpecification : BaseSpecification<Court>
{
    public CourtWithFiltersSpecification(CourtSpecParams specParams)
    {
        AddInclude(c => c.Facility);
        AddInclude(c => c.Sport);

        Expression<Func<Court, bool>> criteria = c => true;

        if (specParams.FacilityId.HasValue)
        {
            criteria = c => c.FacilityId == specParams.FacilityId.Value;
        }

        if (specParams.OwnerId.HasValue)
        {
            Expression<Func<Court, bool>> ownerCriteria = c => c.Facility.OwnerId == specParams.OwnerId.Value;
            criteria = CombineExpressions(criteria, ownerCriteria);
        }

        if (specParams.SportId.HasValue)
        {
            Expression<Func<Court, bool>> sportCriteria = c => c.SportId == specParams.SportId.Value;
            criteria = CombineExpressions(criteria, sportCriteria);
        }

        if (!string.IsNullOrEmpty(specParams.Search))
        {
            Expression<Func<Court, bool>> searchCriteria = c => c.Name.ToLower().Contains(specParams.Search.ToLower());
            criteria = CombineExpressions(criteria, searchCriteria);
        }

        Criteria = criteria;
    }

    private static Expression<Func<Court, bool>> CombineExpressions(
        Expression<Func<Court, bool>> expr1,
        Expression<Func<Court, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(Court));
        var body = Expression.AndAlso(
            Expression.Invoke(expr1, parameter),
            Expression.Invoke(expr2, parameter));
        return Expression.Lambda<Func<Court, bool>>(body, parameter);
    }
}