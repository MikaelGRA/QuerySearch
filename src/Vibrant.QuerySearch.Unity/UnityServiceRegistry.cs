using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace Vibrant.QuerySearch.Unity
{
   public class UnityServiceRegistry : IServiceRegistry
   {
      private readonly UnityContainer _container;

      public UnityServiceRegistry()
      {
         _container = new UnityContainer();
      }

      public UnityServiceRegistry( UnityContainer container )
      {
         _container = container;
      }

      public void RegisterInstance<TService>( TService service )
      {
         _container.RegisterInstance<TService>( service );
      }

      public void RegisterType<TService>( ServiceLifetime lifetime )
      {
         if( lifetime == ServiceLifetime.Singleton )
         {
            _container.RegisterType<TService>( new ContainerControlledLifetimeManager() );
         }
         else if( lifetime == ServiceLifetime.Transient )
         {
            _container.RegisterType<TService>( new TransientLifetimeManager() );
         }
         else
         {
            throw new ArgumentException( nameof( lifetime ) );
         }
      }

      public void RegisterType<TServiceInterface, TServiceImplementation>( ServiceLifetime lifetime )
         where TServiceImplementation : TServiceInterface
      {
         if( lifetime == ServiceLifetime.Singleton )
         {
            _container.RegisterType<TServiceInterface, TServiceImplementation>( new ContainerControlledLifetimeManager() );
         }
         else if( lifetime == ServiceLifetime.Transient )
         {
            _container.RegisterType<TServiceInterface, TServiceImplementation>( new TransientLifetimeManager() );
         }
         else
         {
            throw new ArgumentException( nameof( lifetime ) );
         }
      }

      public TService Resolve<TService>()
      {
         return _container.Resolve<TService>();
      }
   }
}
