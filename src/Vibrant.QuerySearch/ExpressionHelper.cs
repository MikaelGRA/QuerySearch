using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   internal static class ExpressionHelper
   {
      internal static PropertyPathResult CalculatePropertyGetter( ParameterExpression parameter, string path )
      {
         Type currentType = parameter.Type;
         Expression currentExpression = null;
         PropertyInfo propertyInfo = null;

         foreach( var propertyName in path.Split( '.' ) )
         {
            propertyInfo = currentType.GetProperty( propertyName );
            if( propertyInfo == null )
            {
               throw new QuerySearchException( $"Could not find the property '{propertyName}' on the type '{currentType.FullName}'." );
            }
            currentType = propertyInfo.PropertyType;
            currentExpression = Expression.Property( currentExpression ?? parameter, propertyInfo );
         }

         return new PropertyPathResult( currentExpression, propertyInfo.PropertyType );
      }
   }
}
