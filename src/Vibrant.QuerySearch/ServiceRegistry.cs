using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public static class ServiceRegistry
   {
      static ServiceRegistry()
      {
      }

      public static IServiceRegistry Current { get; set; }
   }
}
