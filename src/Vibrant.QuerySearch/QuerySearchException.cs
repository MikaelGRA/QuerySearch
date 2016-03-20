using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// Exception thrown when a query search goes wrong.
   /// </summary>
   public class QuerySearchException : Exception
   {
      /// <summary>
      /// Constructs a QuerySearchException with a message.
      /// </summary>
      /// <param name="message"></param>
      public QuerySearchException( string message ) : base( message ) { }

      /// <summary>
      /// Constructs a QuerySearchException with a message and inner exception.
      /// </summary>
      /// <param name="message"></param>
      /// <param name="inner"></param>
      public QuerySearchException( string message, Exception inner ) : base( message, inner ) { }
   }
}
