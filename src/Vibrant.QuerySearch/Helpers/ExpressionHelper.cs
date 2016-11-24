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
   /// Utility class to make working with expressions easier.
   /// </summary>
   public static class ExpressionHelper
   {
      private static readonly HashSet<string> Descending = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "desc", "descending" };
      private static readonly HashSet<string> Ascending = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "asc", "ascending" };
      private static readonly BindingFlags FindPropertyFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

      /// <summary>
      /// Calculates an expression that accesses the member at the specified property path of the parameter.
      /// </summary>
      /// <param name="parameter">The parameter to access members upon.</param>
      /// <param name="propertyPath">The property path to traverse.</param>
      /// <returns>A MemberAccess representing acessing the property path.</returns>
      public static MemberAccess CalculateMemberAccess( ParameterExpression parameter, string propertyPath )
      {
         Type currentType = parameter.Type;
         Expression currentExpression = null;
         PropertyInfo propertyInfo = null;

         foreach( var propertyName in propertyPath.Split( '.' ) )
         {
            propertyInfo = currentType.GetProperty( propertyName, FindPropertyFlags );
            if( propertyInfo == null )
            {
               throw new QuerySearchException( $"Could not find the property '{propertyName}' on the type '{currentType.FullName}'." );
            }
            currentType = propertyInfo.PropertyType;
            currentExpression = Expression.Property( currentExpression ?? parameter, propertyInfo );
         }

         return new MemberAccess( propertyInfo.PropertyType, currentExpression );
      }

      /// <summary>
      /// Calcutes an expression that access the member at the specified property path of the parameter together with a ordering.
      /// </summary>
      /// <param name="parameter">The parameter to access members upon.</param>
      /// <param name="propertyPathWithOrder">The property path to traverse together with an optional sort direction.</param>
      /// <returns>A SortMemberAccess representing acessing the property path together with a ordering.</returns>
      public static SortMemberAccess CalculateSortMemberAccess( ParameterExpression parameter, string propertyPathWithOrder )
      {
         var sort = SortDirection.Ascending;
         var parts = propertyPathWithOrder.Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
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

         return result.WithSorting( sort );
      }

      /// <summary>
      /// Calcutes an expression that access the members at the specified property paths of the parameter together with a ordering.
      /// </summary>
      /// <param name="parameter">The parameter to access members upon.</param>
      /// <param name="orderBy">A string representing how to sort.</param>
      /// <returns>A enumerable of all sort member accesses calculated.</returns>
      public static IEnumerable<SortMemberAccess> CalculateSortMemberAccesses( ParameterExpression parameter, string orderBy )
      {
         string trimmedOrderBy = orderBy.Trim();
         var allOrderings = trimmedOrderBy.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
         foreach( var propertyPathWithOrdering in allOrderings )
         {
            yield return CalculateSortMemberAccess( parameter, propertyPathWithOrdering.Trim() );
         }
      }

      /// <summary>
      /// Alternative to Expression.Constant, that will be parameterized by 
      /// sql providers, such as Entity Framework.
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
