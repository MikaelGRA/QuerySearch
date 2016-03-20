using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public class PropertyComparison : IMemberComparison
   {
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

      public MemberAccess GetMemberAccess( ParameterExpression parameter )
      {
         return ExpressionHelper.CalculateMemberAccess( parameter, Path );
      }
   }
}
