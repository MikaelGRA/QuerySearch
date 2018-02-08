using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.QuerySearch.Form;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// IPaginationProvider is an implementation that provides pagination for a specific entity.
   /// </summary>
   /// <typeparam name="TEntity">The entity to provide pagination for.</typeparam>
   public interface IPaginationProvider<TEntity>
   {
      /// <summary>
      /// Applies the pagination form to the query.
      /// </summary>
      /// <param name="query">The query that should be paginated.</param>
      /// <param name="form">The pagination form that should be applied to the query.</param>
      /// <returns>A pagination result.</returns>
      PaginationResult<TEntity> ApplyPagination( IQueryable<TEntity> query, IPageForm pageForm );
   }
}
