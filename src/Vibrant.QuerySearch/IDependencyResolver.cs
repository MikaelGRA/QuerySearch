using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// Interface used for resolving dependencies.
   /// </summary>
   public interface IDependencyResolver
   {
      /// <summary>
      /// Resolves an instance of the specified service.
      /// </summary>
      /// <typeparam name="TService"></typeparam>
      /// <returns>An instance of the service to be resolved.</returns>
      TService Resolve<TService>();
   }
}
