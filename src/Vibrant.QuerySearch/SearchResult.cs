using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public class SearchResult<TEntity>
   {
      public int FullCount { get; set; }

      public int FilteredCount { get; set; }

      public int Skip { get; set; }

      public int Take { get; set; }

      public int? FilteredPageCount { get; set; }

      public int? FullPageCount { get; set; }

      public int? Page { get; set; }

      public List<TEntity> Items { get; set; }
   }
}
