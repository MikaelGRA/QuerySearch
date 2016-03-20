using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace Vibrant.QuerySearch.Unity
{
   /// <summary>
   /// Unity implementation of IDependencyResolver using a UnityContainer behind the scenes.
   /// </summary>
   public class UnityDependencyResolver : IDependencyResolver
   {
      private readonly UnityContainer _container;

      /// <summary>
      /// Constructs a UnityDependencyResolver using a UnityContainer.
      /// </summary>
      /// <param name="container"></param>
      public UnityDependencyResolver( UnityContainer container )
      {
         _container = container;
      }

      /// <summary>
      /// Resolves an instance of the specified service.
      /// </summary>
      /// <typeparam name="TService"></typeparam>
      /// <returns>An instance of the service to be resolved.</returns>
      public TService Resolve<TService>()
      {
         return _container.Resolve<TService>();
      }
   }
}
