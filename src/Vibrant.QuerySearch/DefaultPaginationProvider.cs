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
   public class DefaultPaginationProvider<TEntity> : IPaginationProvider<TEntity>
   {
      private dynamic _defaultSort;
      private SortDirection _defaultSortDirection;
      private dynamic _uniqueSort;
      private SortDirection _uniqueSortDirection;
      private ParameterExpression _parameter;

      /// <summary>
      /// Constructs an PagedPaginationProvider with a page size of 20.
      /// </summary>
      public DefaultPaginationProvider()
      {
         PageSize = 20;
         MaxTake = 20;
         PredefinedPageSizes = new HashSet<int>();
         _parameter = Expression.Parameter( typeof( TEntity ), "x" );
      }

      /// <summary>
      /// Gets or sets the PageSize.
      /// </summary>
      public int PageSize { get; set; }

      /// <summary>
      /// Gets or sets the pagination mode.
      /// </summary>
      public PaginationMode Mode { get; set; }

      /// <summary>
      /// Gets the predined page sizes.
      /// </summary>
      public ICollection<int> PredefinedPageSizes { get; private set; }

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
         var page = form.GetPage();
         var skip = form.GetSkip();
         var take = form.GetTake();

         var sorting = form.GetSorting( _parameter )?.ToList();
         if( sorting != null && sorting.Count > 0 )
         {
            query = query.OrderBy( _parameter, sorting );
            if(_uniqueSortDirection == SortDirection.Ascending )
            {
               query = Queryable.ThenBy( (IOrderedQueryable<TEntity>)query, _uniqueSort ?? _defaultSort );
            }
            else
            {
               query = Queryable.ThenByDescending( (IOrderedQueryable<TEntity>)query, _uniqueSort ?? _defaultSort );
            }
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

         if( Mode == PaginationMode.SkipAndTake )
         {
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
         else
         {
            var pageSize = GetPageSize( form );
            int actualPage = 0;
            if( page.HasValue )
            {
               actualPage = page.Value;
            }
            else if( skip.HasValue )
            {
               actualPage = skip.Value / pageSize;
            }

            return new PaginationResult<TEntity>(
               actualPage * pageSize,
               pageSize,
               actualPage,
               pageSize,
               query.Skip( actualPage * pageSize ).Take( pageSize ) );
         }
      }

      private int GetPageSize( IPageForm form )
      {
         switch( Mode )
         {
            case PaginationMode.PageSize:
               return PageSize;
            case PaginationMode.PredefinedPageSizes:
               var pageSize = form.GetPageSize();
               if( !pageSize.HasValue )
               {
                  throw new QuerySearchException( "No page sizes has been specified in the query." );
               }
               if( !PredefinedPageSizes.Contains( pageSize.Value ) )
               {
                  throw new QuerySearchException( "The specified page size is not allowed." );
               }
               return pageSize.Value;
            case PaginationMode.AnyPageSize:
               return form.GetPageSize() ?? PageSize;
            default:
               throw new QuerySearchException( $"Invalid PaginationMode configured: {Mode}." );
         }
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

      /// <summary>
      /// Registers a unique sorting behaviour.
      /// </summary>
      /// <typeparam name="TKey"></typeparam>
      /// <param name="uniqueSort">An expression representing the unique sorting.</param>
      /// <param name="direction">The direction of the default sorting.</param>
      public void RegisterUniqueSort<TKey>( Expression<Func<TEntity, TKey>> uniqueSort, SortDirection direction = SortDirection.Ascending )
      {
         _uniqueSort = uniqueSort;
         _uniqueSortDirection = direction;
      }
   }
}
