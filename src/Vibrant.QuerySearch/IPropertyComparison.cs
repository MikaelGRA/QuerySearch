using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public interface IPropertyComparison
   {
      object GetValue();

      ComparisonType GetComparisonType();

      Expression GetPropertyGetter( ParameterExpression parameter );

      Type GetValueType( ParameterExpression parameter );
   }
}
