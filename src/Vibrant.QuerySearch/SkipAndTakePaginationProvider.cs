using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// An implementation of IPaginationProvider that simply enforces a max take per query.
   /// </summary>
   /// <typeparam name="TEntity"></typeparam>
   public class SkipAndTakePaginationProvider<TEntity> : IPaginationProvider<TEntity>
   {
      private dynamic _defaultSort;
      private SortDirection _defaultSortDirection;
      private ParameterExpression _parameter;

      /// <summary>
      /// Constructs a SkipAndTakePaginationProvider with a max take of 20.
      /// </summary>
      public SkipAndTakePaginationProvider()
      {
         MaxTake = 20;
         _parameter = Expression.Parameter( typeof( TEntity ), "x" );
      }

      /// <summary>
      /// Gets or sets the MaxTake.
      /// </summary>
      public int MaxTake { get; set; }

      /// <summary>
      /// Applies the pagination form to the query.
      /// </summary>
      /// <param name="query">The query that should be paginated.</param>
      /// <param name="form">The pagination form that should be applied to the query.</param>
      /// <returns>A pagination result.</returns>
      public PaginationResult<TEntity> ApplyPagination( IQueryable<TEntity> query, IPageForm form )
      {
         var skip = form.GetSkip();
         var take = form.GetTake();

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

         int actualSkip = 0;
         if( skip.HasValue )
         {
            actualSkip = skip.Value;
         }

         int actualTake = MaxTake;
         if( take.HasValue && take.Value < MaxTake )
         {
            actualTake = MaxTake;
         }

         return new PaginationResult<TEntity>( 
            actualSkip, 
            actualTake, 
            query.Skip( actualSkip ).Take( actualTake ) );
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
