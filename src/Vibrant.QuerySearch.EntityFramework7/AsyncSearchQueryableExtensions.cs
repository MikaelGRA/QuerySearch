using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Entity;

namespace Vibrant.QuerySearch.EntityFramework
{
   public static class AsyncSearchQueryableExtensions
   {
      public async static Task<SearchResult<TEntity>> SearchAsync<TEntity>( this IQueryable<TEntity> query, ISearchForm search )
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

         var fullCount = await query.CountAsync().ConfigureAwait( false );

         query = filterProvider.ApplyWhere( query, search );

         var filteredCount = await query.CountAsync().ConfigureAwait( false );

         var result = paginationProvider.ApplyPagination( query, search );

         var items = await result.Query.ToListAsync().ConfigureAwait( false );

         return new SearchResult<TEntity>
         {
            FullCount = fullCount,
            FilteredCount = filteredCount,
            FullPageCount = PaginationUtilities.GetPageCount( fullCount, result.PageSize ),
            FilteredPageCount = PaginationUtilities.GetPageCount( filteredCount, result.PageSize ),
            Page = result.Page,
            Skip = result.Skip,
            Take = result.Take,
            Items = items
         };
      }
   }
}
