using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public class SkipAndTakePaginationProvider<TEntity> : IPaginationProvider<TEntity>
   {
      private dynamic _defaultSort;
      private ParameterExpression _parameter;

      public SkipAndTakePaginationProvider()
      {
         MaxTake = 20;
         _parameter = Expression.Parameter( typeof( TEntity ), "x" );
      }

      public int MaxTake { get; set; }

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
            query = Queryable.OrderBy( query, _defaultSort );
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

         return new PaginationResult<TEntity>
         {
            Skip = actualSkip,
            Take = actualTake,
            Query = query.Skip( actualSkip ).Take( actualTake ),
         };
      }

      public void RegisterDefaultSort<TKey>( Expression<Func<TEntity, TKey>> defaultSort )
      {
         _defaultSort = defaultSort;
      }
   }
}
