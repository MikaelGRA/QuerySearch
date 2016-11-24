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
         services.AddSingleton<IQuerySearchProvider<Item>, ItemQuerySearchProvider>();
         services.AddSingleton<ItemQuerySearchProvider>();

         var provider = services.BuildServiceProvider();

         DependencyResolver.Current = new DependencyResolverImpl( provider );

         var items = new List<Item>
         {
            new Item { Lol = "Hehehe" },
         };

         var form = new SearchForm
         {
            OrderBy = "lol desc",
            PageSize = 10,
            Parameters = new List<PropertyComparison>
            {
               new PropertyComparison
               {
                  Path = "lol",
                  Type = ComparisonType.Contains,
                  Value = "ehe"
               }
            }
         };

         var result = items.AsQueryable().Search<Item, ItemQuerySearchProvider>( form );

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

   public class ItemQuerySearchProvider : DefaultQuerySearchProvider<Item>
   {
      public ItemQuerySearchProvider( ILocalizationService localization ) : base( localization )
      {
         RegisterDefaultSort( x => x.Lol, SortDirection.Ascending, true );
         Mode = PaginationMode.MinMaxPageSize;
      }
   }

   public class Item
   {
      public string Lol { get; set; }
   }
}
