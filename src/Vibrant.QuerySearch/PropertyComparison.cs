using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public class PropertyComparison : IPropertyComparison
   {
      private Type _valueType;
      private Expression _propertyGetter;

      public string Path { get; set; }

      public object Value { get; set; }

      public ComparisonType Type { get; set; }

      public object GetValue()
      {
         return Value;
      }

      public ComparisonType GetComparisonType()
      {
         return Type;
      }

      public Expression GetPropertyGetter( ParameterExpression parameter )
      {
         ReflectPropertyPath( parameter );

         return _propertyGetter;
      }

      public Type GetValueType( ParameterExpression parameter )
      {
         ReflectPropertyPath( parameter );

         return _valueType;
      }

      private void ReflectPropertyPath( ParameterExpression parameter )
      {
         if( _propertyGetter == null )
         {
            var result = ExpressionHelper.CalculatePropertyGetter( parameter, Path );

            _propertyGetter = result.PropertyGetter;
            _valueType = result.PropertyType;
         }
      }
   }
}
