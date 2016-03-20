using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// An implenentation of IPaginationProvider that forces exact pagination of result sets.
   /// </summary>
   /// <typeparam name="TEntity"></typeparam>
   public class PagedPaginationProvider<TEntity> : IPaginationProvider<TEntity>
   {
      private dynamic _defaultSort;
      private SortDirection _defaultSortDirection;
      private ParameterExpression _parameter;

      /// <summary>
      /// Constructs an PagedPaginationProvider with a page size of 20.
      /// </summary>
      public PagedPaginationProvider()
      {
         PageSize = 20;
         _parameter = Expression.Parameter( typeof( TEntity ), "x" );
      }

      /// <summary>
      /// Gets or sets the PageSize.
      /// </summary>
      public int PageSize { get; set; }

      /// <summary>
      /// Applies the pagination form to the query.
      /// </summary>
      /// <param name="query">The query that should be paginated.</param>
      /// <param name="form">The pagination form that should be applied to the query.</param>
      /// <returns>A pagination result.</returns>
      public PaginationResult<TEntity> ApplyPagination( IQueryable<TEntity> query, IPageForm form )
      {
         var page = form.GetPage();
         var skip = form.GetSkip();

         var sorting = form.GetSorting( _parameter )?.ToList();
         if( sorting != null && sorting.Count > 0 )
         {
            query = query.OrderBy( _parameter, sorting );
         }
         else
         {
            if( _defaultSortDirection == SortDirection.Ascending )
            {
               query = Queryable.OrderBy( query, _defaultSort );
            }
            else
            {
               query = Queryable.OrderByDescending( query, _defaultSort );
            }
         }

         int actualPage = 0;
         if( page.HasValue )
         {
            actualPage = page.Value;
         }
         else if( skip.HasValue )
         {
            actualPage = skip.Value / PageSize;
         }

         return new PaginationResult<TEntity>(
            actualPage * PageSize,
            PageSize,
            actualPage,
            PageSize,
            query.Skip( actualPage * PageSize ).Take( PageSize ) );
      }

      /// <summary>
      /// Registers the default sorting behaviour.
      /// </summary>
      /// <typeparam name="TKey"></typeparam>
      /// <param name="defaultSort">An expression representing the default sorting.</param>
      /// <param name="direction">The direction of the default sorting.</param>
      public void RegisterDefaultSort<TKey>( Expression<Func<TEntity, TKey>> defaultSort, SortDirection direction = SortDirection.Ascending )
      {
         _defaultSort = defaultSort;
         _defaultSortDirection = direction;
      }
   }
}
