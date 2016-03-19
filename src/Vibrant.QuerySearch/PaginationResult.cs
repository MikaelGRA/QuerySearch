using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public class PaginationResult<TEntity>
   {
      public int Skip { get; set; }

      public int Take { get; set; }

      public int? Page { get; set; }

      public int? PageSize { get; set; }

      public IQueryable<TEntity> Query { get; set; }
   }
}
