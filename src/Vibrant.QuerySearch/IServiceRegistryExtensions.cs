using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public static class IServiceRegistryExtensions
   {
      public static void RegisterLocalizationService<TLocalizationService>(
         this IServiceRegistry registry,
         ServiceLifetime lifetime = ServiceLifetime.Singleton )
         where TLocalizationService : ILocalizationService
      {
         registry.RegisterType<ILocalizationService, TLocalizationService>( lifetime );
      }

      public static void RegisterFilterProvider<TEntity, TFilterProvider>(
         this IServiceRegistry registry,
         ServiceLifetime lifetime = ServiceLifetime.Singleton )
         where TFilterProvider : IFilterProvider<TEntity>
      {
         registry.RegisterType<IFilterProvider<TEntity>, TFilterProvider>( lifetime );
      }

      public static void RegisterPaginationProvider<TEntity, TPaginationProvider>(
         this IServiceRegistry registry,
         ServiceLifetime lifetime = ServiceLifetime.Singleton )
         where TPaginationProvider : IPaginationProvider<TEntity>
      {
         registry.RegisterType<IPaginationProvider<TEntity>, TPaginationProvider>( lifetime );
      }
   }
}
