using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public static class DependencyResolver
   {
      static DependencyResolver()
      {
      }

      public static IDependencyResolver Current { get; set; }
   }
}
