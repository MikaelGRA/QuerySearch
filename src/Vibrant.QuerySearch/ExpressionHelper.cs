using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public static class ExpressionHelper
   {
      private static readonly HashSet<string> Descending = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "desc", "descending" };
      private static readonly HashSet<string> Ascending = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "asc", "ascending" };
      
      public static MemberAccess CalculateMemberAccess( ParameterExpression parameter, string propertyPath )
      {
         Type currentType = parameter.Type;
         Expression currentExpression = null;
         PropertyInfo propertyInfo = null;

         foreach( var propertyName in propertyPath.Split( '.' ) )
         {
            propertyInfo = currentType.GetProperty( propertyName );
            if( propertyInfo == null )
            {
               throw new QuerySearchException( $"Could not find the property '{propertyName}' on the type '{currentType.FullName}'." );
            }
            currentType = propertyInfo.PropertyType;
            currentExpression = Expression.Property( currentExpression ?? parameter, propertyInfo );
         }

         return new MemberAccess( propertyInfo.PropertyType, currentExpression );
      }

      public static IEnumerable<SortMemberAccess> CalculateSortMemberAccess( ParameterExpression parameter, string orderBy )
      {
         string trimmedOrderBy = orderBy.Trim();

         var allOrderings = trimmedOrderBy.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
         foreach( var order in allOrderings )
         {
            var trimmedOrder = order.Trim();

            var sort = SortDirection.Ascending;
            var parts = trimmedOrder.Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            var propertyPath = parts[ 0 ]; // automatically trimmed due to split on whitespace
            if( parts.Length >= 2 )
            {
               string ordering = parts[ 1 ]; // automatically trimmed due to split on whitespace

               if( Ascending.Contains( ordering ) )
               {
                  sort = SortDirection.Ascending;
               }
               else if( Descending.Contains( ordering ) )
               {
                  sort = SortDirection.Descending;
               }
               else
               {
                  throw new QuerySearchException( $"Could not determine sort direction from the text: '{ordering}'." );
               }
            }

            // create property getter for path...
            var result = CalculateMemberAccess( parameter, propertyPath );

            yield return result.WithSorting( sort );
         }
      }

      /// <summary>
      /// http://graemehill.ca/entity-framework-dynamic-queries-and-parameterization/
      /// </summary>
      public static MemberExpression WrappedConstant( Type type, object value )
      {
         var genericWrapperType = typeof( WrappedObj<> ).MakeGenericType( type );

         var wrapper = genericWrapperType
            .GetConstructor( new[] { type } )
            .Invoke( new[] { value } );

         return Expression.Property(
             Expression.Constant( wrapper ),
             genericWrapperType.GetProperty( "Value" ) );
      }

      private class WrappedObj<TValue>
      {
         public TValue Value { get; set; }

         public WrappedObj( TValue value )
         {
            Value = value;
         }
      }
   }
}
