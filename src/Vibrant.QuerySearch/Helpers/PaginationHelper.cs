using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// Simple class that can be used to work with pagination.
   /// </summary>
   public class PaginationHelper
   {
      /// <summary>
      /// Gets the number of pages from a count and a page size.
      /// </summary>
      /// <param name="count">The entry count.</param>
      /// <param name="pageSize">The size of a page.</param>
      /// <returns>The number of pages.</returns>
      public static int? GetPageCount( int count, int? pageSize )
      {
         if( !pageSize.HasValue )
         {
            return null;
         }

         if( count % pageSize.Value == 0 )
         {
            return count / pageSize;
         }
         else
         {
            return ( count / pageSize ) + 1;
         }
      }
   }
}
