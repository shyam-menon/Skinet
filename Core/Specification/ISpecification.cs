using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Core.Specification
{
    public interface ISpecification<T>
    {
        //Filtering
         Expression<Func<T, bool>> Criteria {get; }
         //Include related entities
         List<Expression<Func<T, object>>> Includes {get; }

         //Ordering
         Expression<Func<T, object>> OrderBy {get; }
         Expression<Func<T, object>> OrderByDescending {get; }

         //Paging
         int Take {get;}
         int Skip {get;}
         bool IsPagingEnabled {get;}

    }
}