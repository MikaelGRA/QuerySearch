using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public interface IServiceRegistry
   {
      TService Resolve<TService>();

      void RegisterType<TServiceInterface, TServiceImplementation>( ServiceLifetime lifetime ) where TServiceImplementation : TServiceInterface;

      void RegisterType<TService>( ServiceLifetime lifetime );

      void RegisterInstance<TService>( TService service );
   }
}
