using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{

   public class QuerySearchException : Exception
   {
      public QuerySearchException() { }
      public QuerySearchException( string message ) : base( message ) { }
      public QuerySearchException( string message, Exception inner ) : base( message, inner ) { }
   }
}
