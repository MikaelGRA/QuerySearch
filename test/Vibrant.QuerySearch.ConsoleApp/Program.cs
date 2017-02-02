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
         var id = Guid.NewGuid();

         var items = new List<Item>
         {
            new Item { Lol = "Hehehe", Id = id },
         };

         var form = new SearchForm
         {
            OrderBy = "lol desc, lol desc",
            PageSize = 10,
            ParameterComposition = FilterComposition.Or,
            Parameters = new List<PropertyComparison>
            {
               new PropertyComparison
               {
                  Path = "lol",
                  Type = ComparisonType.Contains,
                  Value = "aehe"
               },
               new PropertyComparison
               {
                  Path = "Id",
                  Type = ComparisonType.Equal,
                  Value = Guid.NewGuid()
               },
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
         RegisterDefaultSort( q => q.OrderBy( x => x.Lol ), true );
         RegisterUniqueSort( q => q.ThenBy( x => x.Lol ) );
         Mode = PaginationMode.MinMaxPageSize;
      }
   }

   public class Item
   {
      public string Lol { get; set; }

      public Guid Id { get; set; }
   }
}
