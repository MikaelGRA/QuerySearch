using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Vibrant.QuerySearch.Unity;
using Xunit;

namespace Vibrant.QuerySearch.Tests
{
   public class MyObjectFilterProvider : DefaultFilterProvider<MyObject>
   {
      public MyObjectFilterProvider( ILocalizationService localization ) : base( localization )
      {
         RegisterWordSearch( s => x => x.Text.Contains( s ) );
      }
   }

   public class MyObjectPaginationProvider : DefaultPaginationProvider<MyObject>
   {
      public MyObjectPaginationProvider(
         Expression<Func<MyObject, object>> defaultSort,
         Expression<Func<MyObject, object>> uniqueSort )
      {
         Mode = PaginationMode.AnyPageSize;

         RegisterDefaultSort( defaultSort );
         RegisterUniqueSort( uniqueSort );
      }

      public MyObjectPaginationProvider( Expression<Func<MyObject, object>> defaultSort )
      {
         Mode = PaginationMode.AnyPageSize;

         RegisterDefaultSort( defaultSort, SortDirection.Ascending, true );
      }
   }

   public class QuerySearchTests
   {
      public QuerySearchTests()
      {

      }

      private List<MyObject> CreateCollection()
      {
         return new List<MyObject>
         {
            new MyObject { Id = 1, Text = "Hello World" },
            new MyObject { Id = 2, Text = "Hejsa" },
            new MyObject { Id = 3, Text = "Hej" },
            new MyObject { Id = 4, Text = "Ni hao" },
            new MyObject { Id = 5, Text = "Goddag" },
            new MyObject { Id = 6, Text = "Hello World" },
         };
      }

      private void BootstrapDependencies( Expression<Func<MyObject, object>> defaultSort, Expression<Func<MyObject, object>> uniqueSort = null )
      {
         var container = new UnityContainer();
         var localization = new DefaultLocalizationService();

         container.RegisterInstance<ILocalizationService>( localization );
         container.RegisterInstance<IFilterProvider<MyObject>>( new MyObjectFilterProvider( localization ) );
         if( uniqueSort == null )
         {
            container.RegisterInstance<IPaginationProvider<MyObject>>( new MyObjectPaginationProvider( defaultSort ) );
         }
         else
         {
            container.RegisterInstance<IPaginationProvider<MyObject>>( new MyObjectPaginationProvider( defaultSort, uniqueSort ) );
         }

         DependencyResolver.Current = new UnityDependencyResolver( container );
      }

      [Theory]
      [InlineData( 0, 3 )]
      [InlineData( 1, 3 )]
      public void Should_Be_Sorted_Correctly_With_Default_Sort( int page, int pageSize )
      {
         // Arrange
         BootstrapDependencies( x => x.Text, x => x.Id );
         var collection = CreateCollection().AsQueryable();
         var expected = CreateCollection()
            .OrderBy( x => x.Text )
            .ThenBy( x => x.Id )
            .Skip( page * pageSize )
            .Take( pageSize )
            .ToList();

         // Act
         var result = collection.Search( new SearchForm
         {
            Page = page,
            PageSize = pageSize,
         } );

         // Assert
         Assert.Equal( expected.Count, result.Items.Count );
         for( int i = 0 ; i < expected.Count ; i++ )
         {
            var expectedItem = expected[ i ];
            var actualItem = result.Items[ i ];

            Assert.Equal( expectedItem.Id, actualItem.Id );
            Assert.Equal( expectedItem.Text, actualItem.Text );
         }
      }

      [Theory]
      [InlineData( 0, 3 )]
      [InlineData( 1, 3 )]
      public void Should_Be_Sorted_Correctly_With_Text_Sort( int page, int pageSize )
      {
         // Arrange
         BootstrapDependencies( x => x.Id );
         var collection = CreateCollection().AsQueryable();
         var expected = CreateCollection()
            .OrderByDescending( x => x.Text )
            .ThenBy( x => x.Id )
            .Skip( page * pageSize )
            .Take( pageSize )
            .ToList();

         // Act
         var result = collection.Search( new SearchForm
         {
            Page = page,
            PageSize = pageSize,
            OrderBy = "Text DESC"
         } );

         // Assert
         Assert.Equal( expected.Count, result.Items.Count );
         for( int i = 0 ; i < expected.Count ; i++ )
         {
            var expectedItem = expected[ i ];
            var actualItem = result.Items[ i ];

            Assert.Equal( expectedItem.Id, actualItem.Id );
            Assert.Equal( expectedItem.Text, actualItem.Text );
         }
      }
   }
}
