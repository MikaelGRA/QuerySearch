using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public static class DynamicQueryable
   {
      private static readonly HashSet<string> Descending = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "desc", "descending" };
      private static readonly HashSet<string> Ascending = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "asc", "ascending" };
      private static readonly MethodInfo OrderByMethod = typeof( Queryable ).GetMethods().Single( x => x.Name == "OrderBy" && x.IsGenericMethodDefinition == true && x.GetGenericArguments().Length == 2 && x.GetParameters().Length == 2 );
      private static readonly MethodInfo OrderByDescendingMethod = typeof( Queryable ).GetMethods().Single( x => x.Name == "OrderByDescending" && x.IsGenericMethodDefinition == true && x.GetGenericArguments().Length == 2 && x.GetParameters().Length == 2 );
      private static readonly MethodInfo ThenByMethod = typeof( Queryable ).GetMethods().Single( x => x.Name == "ThenBy" && x.IsGenericMethodDefinition == true && x.GetGenericArguments().Length == 2 && x.GetParameters().Length == 2 );
      private static readonly MethodInfo ThenByDescendingMethod = typeof( Queryable ).GetMethods().Single( x => x.Name == "ThenByDescending" && x.IsGenericMethodDefinition == true && x.GetGenericArguments().Length == 2 && x.GetParameters().Length == 2 );

      public static IQueryable<TEntity> OrderBy<TEntity>( this IQueryable<TEntity> query, string orderBy )
      {
         var parameter = Expression.Parameter( typeof( TEntity ), "x" );

         // Create expression to sort by
         bool hasSorted = false;

         var allOrderings = orderBy.Split( ',' );
         foreach( var order in allOrderings )
         {
            bool isDescending = false;

            var parts = order.Split( ' ' );
            string propertyPath = parts[ 0 ];
            if( parts.Length >= 2 )
            {
               string ordering = parts[ 1 ];

               if( Ascending.Contains( ordering ) )
               {
                  isDescending = false;
               }
               else if( Descending.Contains( ordering ) )
               {
                  isDescending = true;
               }
               else
               {
                  throw new QuerySearchException( $"Could not determine sort direction from the text: '{ordering}'." );
               }
            }

            // create property getter for path...
            var result = ExpressionHelper.CalculatePropertyGetter( parameter, propertyPath );
            var delegateType = typeof( Func<,> ).MakeGenericType( typeof( TEntity ), result.PropertyType );
            var lambda = Expression.Lambda( delegateType, result.PropertyGetter, parameter );

            MethodInfo orderingMethod;
            if( !hasSorted )
            {
               hasSorted = true;
               if( isDescending )
               {
                  orderingMethod = OrderByDescendingMethod;
               }
               else
               {
                  orderingMethod = OrderByMethod;
               }
            }
            else
            {
               if( isDescending )
               {
                  orderingMethod = ThenByDescendingMethod;
               }
               else
               {
                  orderingMethod = ThenByMethod;
               }
            }

            query = (IQueryable<TEntity>)orderingMethod.MakeGenericMethod( typeof( TEntity ), result.PropertyType ).Invoke( null, new object[] { query, lambda } );
         }

         return query;
      }
   }
}
