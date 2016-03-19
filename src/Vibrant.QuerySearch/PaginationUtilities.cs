using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public class PaginationUtilities
   {
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
