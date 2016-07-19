using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Vibrant.QuerySearch.EntityFrameworkCore
{
   /// <summary>
   /// Provides additional searching methods for queryables with entity framework 6.
   /// </summary>
   public static class AsyncSearchQueryableExtensions
   {
      /// <summary>
      /// Searches the specfied queryable for entities using the provided search form.
      /// </summary>
      /// <typeparam name="TEntity">The type of entities to look for.</typeparam>
      /// <param name="query">The queryable that should be searched.</param>
      /// <param name="search">The search form that should be used to search the query.</param>
      /// <returns>A QuerySearchResult that contains the result of the search.</returns>
      public static Task<QuerySearchResult<TEntity>> SearchAsync<TEntity>( this IQueryable<TEntity> query, ISearchForm search )
      {
         return query.SearchAsync( search, null );
      }


      /// <summary>
      /// Searches the specfied queryable for entities using the provided search form.
      /// </summary>
      /// <typeparam name="TEntity">The type of entities to look for.</typeparam>
      /// <param name="query">The queryable that should be searched.</param>
      /// <param name="search">The search form that should be used to search the query.</param>
      /// <param name="postCountProcessesing">Function to be called after processing the count portions of the query. Often used to add navigation inclusions.</param>
      /// <returns>A QuerySearchResult that contains the result of the search.</returns>
      public static async Task<QuerySearchResult<TEntity>> SearchAsync<TEntity>( this IQueryable<TEntity> query, ISearchForm search, Func<IQueryable<TEntity>, IQueryable<TEntity>> postCountProcessesing )
      {
         if( query == null )
            throw new ArgumentNullException( nameof( query ) );
         if( search == null )
            throw new ArgumentNullException( nameof( query ) );

         var registry = DependencyResolver.Current;
         if( registry == null )
            throw new QuerySearchException( "No dependency resolver has been specified." );

         var filterProvider = DependencyResolver.Current.Resolve<IFilterProvider<TEntity>>();
         if( filterProvider == null )
            throw new QuerySearchException( $"No IFilterProvider has been registered for the type '{typeof( TEntity ).FullName}'." );

         var paginationProvider = DependencyResolver.Current.Resolve<IPaginationProvider<TEntity>>();
         if( paginationProvider == null )
            throw new QuerySearchException( $"No IPaginationProvider has been registered for the type '{typeof( TEntity ).FullName}'." );

         var fullCount = await query.CountAsync().ConfigureAwait( false );

         query = filterProvider.ApplyWhere( query, search );

         var filteredCount = await query.CountAsync().ConfigureAwait( false );

         if( postCountProcessesing != null )
         {
            query = postCountProcessesing( query );
         }

         var result = paginationProvider.ApplyPagination( query, search );

         var items = await result.Query.ToListAsync().ConfigureAwait( false );

         return new QuerySearchResult<TEntity>
         {
            FullCount = fullCount,
            FilteredCount = filteredCount,
            FullPageCount = PaginationHelper.GetPageCount( fullCount, result.Page ),
            FilteredPageCount = PaginationHelper.GetPageCount( filteredCount, result.Page ),
            Page = result.Page,
            Skip = result.Skip,
            Take = result.Take,
            Items = items
         };
      }
   }
}
