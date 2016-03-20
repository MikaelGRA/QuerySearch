using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// Class representing the result of applying pagination to a queryable.
   /// </summary>
   /// <typeparam name="TEntity"></typeparam>
   public class PaginationResult<TEntity>
   {
      /// <summary>
      /// Constructs a PaginationResult from skip, take and the query.
      /// </summary>
      /// <param name="skip"></param>
      /// <param name="take"></param>
      /// <param name="query"></param>
      public PaginationResult( int skip, int take, IQueryable<TEntity> query )
      {
         Skip = skip;
         Take = take;
         Query = query;
      }

      /// <summary>
      /// Constructs a PaginationResult from skip, take, page, pageSize and the query.
      /// </summary>
      /// <param name="skip"></param>
      /// <param name="take"></param>
      /// <param name="page"></param>
      /// <param name="pageSize"></param>
      /// <param name="query"></param>
      public PaginationResult( int skip, int take, int? page, int? pageSize, IQueryable<TEntity> query )
      {
         Skip = skip;
         Take = take;
         Page = page;
         PageSize = pageSize;
         Query = query;
      }

      /// <summary>
      /// Gets the number of entries that was skipped.
      /// </summary>
      public int Skip { get; private set; }

      /// <summary>
      /// Gets the number of entries that was taken.
      /// </summary>
      public int Take { get; private set; }

      /// <summary>
      /// Gets the page number, if present.
      /// </summary>
      public int? Page { get; private set; }

      /// <summary>
      /// Gets the page size, if present.
      /// </summary>
      public int? PageSize { get; private set; }

      /// <summary>
      /// Gets the query itself.
      /// </summary>
      public IQueryable<TEntity> Query { get; private set; }
   }
}
