using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using backend.Core.Entities;

namespace backend.Core.Specification;

public interface ISpecification<T> where T : BaseEntity
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; } // For nested includes using string paths
}