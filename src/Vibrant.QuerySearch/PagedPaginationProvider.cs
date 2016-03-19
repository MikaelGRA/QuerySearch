using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Vibrant.QuerySearch
{
   public class PagedPaginationProvider<TEntity> : IPaginationProvider<TEntity>
   {
      private dynamic _defaultSort;

      public PagedPaginationProvider()
      {
         PageSize = 20;
      }

      public int PageSize { get; set; }

      public PaginationResult<TEntity> ApplyPagination( IQueryable<TEntity> query, IPageForm form )
      {
         var orderBy = form.GetOrderBy();
         var page = form.GetPage();
         var skip = form.GetSkip();

         if( orderBy != null )
         {
            query = query.OrderBy( orderBy );
         }
         else
         {
            query = Queryable.OrderBy( query, _defaultSort );
         }

         int actualSkip = 0;
         if( page.HasValue )
         {
            actualSkip = page.Value;
         }
         else if( skip.HasValue )
         {
            actualSkip = skip.Value / PageSize;
         }

         return new PaginationResult<TEntity>
         {
            Page = actualSkip,
            PageSize = PageSize,
            Skip = actualSkip * PageSize,
            Take = PageSize,
            Query = query.Skip( actualSkip * PageSize ).Take( PageSize ),
         };
      }

      public void RegisterDefaultSort<TKey>( Expression<Func<TEntity, TKey>> defaultSort )
      {
         _defaultSort = defaultSort;
      }
   }
}
