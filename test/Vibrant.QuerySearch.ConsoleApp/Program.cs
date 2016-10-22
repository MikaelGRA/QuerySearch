using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vibrant.QuerySearch.EntityFrameworkCore;

namespace Vibrant.QuerySearch.ConsoleApp
{
   public class Program
   {
      public static void Main( string[] args )
      {
         var services = new ServiceCollection();

         services.AddSingleton<ILocalizationService, DefaultLocalizationService>();

         var provider = services.BuildServiceProvider();

         DependencyResolver.Current = new DependencyResolverImpl( provider );

         Console.ReadKey();
      }
   }

   public class DependencyResolverImpl : IDependencyResolver
   {
      private IServiceProvider _serviceProvider;

      public DependencyResolverImpl( IServiceProvider serviceProvider )
      {
         _serviceProvider = serviceProvider;
      }

      public TService Resolve<TService>()
      {
         return _serviceProvider.GetRequiredService<TService>();
      }
   }
}
