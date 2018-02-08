using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vibrant.QuerySearch.Form;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// IFilterProvider is an implementation that provides filtering for a specific entity.
   /// </summary>
   /// <typeparam name="TEntity">The entity to provide filtering for.</typeparam>
   public interface IFilterProvider<TEntity>
   {
      /// <summary>
      /// Applies the filtering form to the query.
      /// </summary>
      /// <param name="query">The query that should be filtered.</param>
      /// <param name="form">The filtering form that should be applied to the query.</param>
      /// <returns>A filtered queryable.</returns>
      IQueryable<TEntity> ApplyWhere( IQueryable<TEntity> query, IFilterForm form );
   }
}
