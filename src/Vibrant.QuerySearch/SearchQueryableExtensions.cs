using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public static class SearchQueryableExtensions
   {
      public static SearchResult<TEntity> Search<TEntity>( this IQueryable<TEntity> query, ISearchForm search )
      {
         if( query == null )
            throw new ArgumentNullException( nameof( query ) );
         if( search == null )
            throw new ArgumentNullException( nameof( query ) );

         var registry = ServiceRegistry.Current;
         if( registry == null )
            throw new QuerySearchException( "No service registry has been specified." );

         var filterProvider = ServiceRegistry.Current.Resolve<IFilterProvider<TEntity>>();
         if( filterProvider == null )
            throw new QuerySearchException( $"No IFilterProvider has been registered for the type '{typeof( TEntity ).FullName}'." );

         var paginationProvider = ServiceRegistry.Current.Resolve<IPaginationProvider<TEntity>>();
         if( paginationProvider == null )
            throw new QuerySearchException( $"No IPaginationProvider has been registered for the type '{typeof( TEntity ).FullName}'." );

         var fullCount = query.Count();

         query = filterProvider.ApplyWhere( query, search );

         var filteredCount = query.Count();

         var result = paginationProvider.ApplyPagination( query, search );

         var items = result.Query.ToList();

         return new SearchResult<TEntity>
         {
            FullCount = fullCount,
            FilteredCount = filteredCount,
            FullPageCount = PaginationUtilities.GetPageCount( fullCount, result.Page ),
            FilteredPageCount = PaginationUtilities.GetPageCount( filteredCount, result.Page ),
            Page = result.Page,
            Skip = result.Skip,
            Take = result.Take,
            Items = items
         };
      }
   }
}
