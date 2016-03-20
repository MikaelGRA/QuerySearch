using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// DynamicQueryable provides extensions useful extension methods for IQueryable.
   /// </summary>
   public static class DynamicQueryable
   {
      private static readonly MethodInfo OrderByMethod = typeof( Queryable ).GetMethods().Single( x => x.Name == "OrderBy" && x.IsGenericMethodDefinition == true && x.GetGenericArguments().Length == 2 && x.GetParameters().Length == 2 );
      private static readonly MethodInfo OrderByDescendingMethod = typeof( Queryable ).GetMethods().Single( x => x.Name == "OrderByDescending" && x.IsGenericMethodDefinition == true && x.GetGenericArguments().Length == 2 && x.GetParameters().Length == 2 );
      private static readonly MethodInfo ThenByMethod = typeof( Queryable ).GetMethods().Single( x => x.Name == "ThenBy" && x.IsGenericMethodDefinition == true && x.GetGenericArguments().Length == 2 && x.GetParameters().Length == 2 );
      private static readonly MethodInfo ThenByDescendingMethod = typeof( Queryable ).GetMethods().Single( x => x.Name == "ThenByDescending" && x.IsGenericMethodDefinition == true && x.GetGenericArguments().Length == 2 && x.GetParameters().Length == 2 );

      /// <summary>
      /// Sorts the queryable by the specified sort member accesses.
      /// </summary>
      /// <typeparam name="TEntity"></typeparam>
      /// <param name="query">The query to be ordered.</param>
      /// <param name="parameter">The parameter that was used to create the sort member accesses.</param>
      /// <param name="sortMemberAccesses">The sort member accesses that the query will be sorted by.</param>
      /// <returns>An ordered queryable.</returns>
      public static IQueryable<TEntity> OrderBy<TEntity>( this IQueryable<TEntity> query, ParameterExpression parameter, IEnumerable<SortMemberAccess> sortMemberAccesses )
      {
         bool hasSorted = false;
         foreach( var sortMemberAccess in sortMemberAccesses )
         {
            var delegateType = typeof( Func<,> ).MakeGenericType( typeof( TEntity ), sortMemberAccess.MemberType );
            var lambda = Expression.Lambda( delegateType, sortMemberAccess.MemberAccessor, parameter );

            MethodInfo orderingMethod;
            if( !hasSorted )
            {
               hasSorted = true;
               if( sortMemberAccess.SortDirection == SortDirection.Descending )
               {
                  orderingMethod = OrderByDescendingMethod;
               }
               else if( sortMemberAccess.SortDirection == SortDirection.Ascending )
               {
                  orderingMethod = OrderByMethod;
               }
               else
               {
                  throw new QuerySearchException( $"Unrecognized sort direction enum value: '{sortMemberAccess.SortDirection}'." );
               }
            }
            else
            {
               if( sortMemberAccess.SortDirection == SortDirection.Descending )
               {
                  orderingMethod = ThenByDescendingMethod;
               }
               else if( sortMemberAccess.SortDirection == SortDirection.Ascending )
               {
                  orderingMethod = ThenByMethod;
               }
               else
               {
                  throw new QuerySearchException( $"Unrecognized sort direction enum value: '{sortMemberAccess.SortDirection}'." );
               }
            }

            query = (IQueryable<TEntity>)orderingMethod.MakeGenericMethod( typeof( TEntity ), sortMemberAccess.MemberType ).Invoke( null, new object[] { query, lambda } );
         }

         return query;
      }

      public static IQueryable<TEntity> OrderBy<TEntity>( this IQueryable<TEntity> query, string orderBy )
      {
         var parameter = Expression.Parameter( typeof( TEntity ), "x" );

         return OrderBy( query, parameter, ExpressionHelper.CalculateSortMemberAccesses( parameter, orderBy ) );
      }
   }
}
