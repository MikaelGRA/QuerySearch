using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public interface IPaginationProvider<TEntity>
   {
      PaginationResult<TEntity> ApplyPagination( IQueryable<TEntity> query, IPageForm pageForm );
   }
}
