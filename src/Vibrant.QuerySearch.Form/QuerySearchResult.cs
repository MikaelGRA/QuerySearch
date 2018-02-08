using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch.Form
{
   /// <summary>
   /// The result of searching for entities.
   /// </summary>
   /// <typeparam name="TEntity"></typeparam>
   public class QuerySearchResult<TEntity>
   {
      /// <summary>
      /// Gets or sets the filtered count of entities.
      /// </summary>
      public int FilteredCount { get; set; }

      /// <summary>
      /// Gets or sets the count of entities that were skipped.
      /// </summary>
      public int Skip { get; set; }

      /// <summary>
      /// Gets or sets the count of entities that were taken.
      /// </summary>
      public int Take { get; set; }

      /// <summary>
      /// Gets or sets the number of pages available in the filtered count.
      /// </summary>
      public int? FilteredPageCount { get; set; }

      /// <summary>
      /// Gets or sets the current page number.
      /// </summary>
      public int? Page { get; set; }

      /// <summary>
      /// Gets a list of the entities that were retrieved.
      /// </summary>
      public List<TEntity> Items { get; set; }
   }
}
