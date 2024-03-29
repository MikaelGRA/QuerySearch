﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Vibrant.QuerySearch.EntityFrameworkCore;
using Vibrant.QuerySearch.Form;

namespace Vibrant.QuerySearch.ConsoleApp
{
   public class Program
   {
      public static void Main( string[] args )
      {
         var services = new ServiceCollection();

         services.AddSingleton<ILocalizationService, DefaultLocalizationService>();
         services.AddSingleton<IQuerySearchProvider<Item>, ItemQuerySearchProvider>();
         services.AddSingleton<IQuerySearchProvider<MyClass>, MyClassQuerySearchProvider>();
         services.AddSingleton<ItemQuerySearchProvider>();

         var provider = services.BuildServiceProvider();

         DependencyResolver.Current = new DependencyResolverImpl( provider );

         //var factory = new MyDbContextFactory();
         //using (var ctx = factory.CreateDbContext(args))
         //{
         //    var term = "This is english";
         //    var termReversed = term.Reverse();
         //    var output = new string(termReversed.ToArray());

         //    //var c = new MyClass() { SomeText = $"{term}. {output}" };
         //    //var cf = new MyRefClass { MyClass = c, Whatver = ":):):)" };
         //    //ctx.MyClasses.Add(c);
         //    //ctx.MyRefClasses.Add(cf);
         //    //ctx.MyClasses.Add(new MyClass() { SomeText = "Dette er dansk!" });

         //    ctx.SaveChanges();

         //    var form = new SearchForm
         //    {
         //        PageSize = 100,
         //        OrderByTerm = true,
         //        Term = "english"
         //    };

         //    var result = ctx.MyClasses.Search(form, q => q.Include(x => x.MyRefClass));

         //    Console.ReadKey();
         //}





         var id = Guid.NewGuid();

         var items = new List<Item>
            {
               new Item { Lol = "Hehehe", Id = id },
               new Item { Lol = "ABC", Id = Guid.NewGuid(), Hehe = Whatever.Haha },
            };

         var form = new SearchForm
         {
            OrderBy = "lol desc",
            PageSize = 10,
            Parameters = new List<PropertyComparison>
            {
               new PropertyComparison
               {
                  Path = "Hehe",
                  Type = ComparisonType.IsAnyOf,
                  Value = new List<object> { "Haha" }
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
         RegisterDefaultSort( q => q.OrderBy( x => x.Lol ) );
         RegisterUniqueSort( q => q.ThenBy( x => x.Lol ) );
         Mode = PaginationMode.MinMaxPageSize;
      }
   }

   public class Item
   {
      public string Lol { get; set; }

      public Guid Id { get; set; }

      public Whatever Hehe { get; set; }
   }

   public enum Whatever
   {
      Hoho,
      Haha
   }
}
