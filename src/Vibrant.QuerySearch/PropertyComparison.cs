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
   /// Implementation of IMemberComparison that can compare a property path to a argument value.
   /// </summary>
   public class PropertyComparison : IMemberComparison
   {
      /// <summary>
      /// Gets or sets the property path.
      /// </summary>
      public string Path { get; set; }

      /// <summary>
      /// Gets or sets the argument value.
      /// </summary>
      public object Value { get; set; }

      /// <summary>
      /// Gets or sets the comparison type.
      /// </summary>
      public ComparisonType Type { get; set; }

      /// <summary>
      /// Gets the argument to compare with member with.
      /// </summary>
      /// <returns>The argument to compare with member with.</returns>
      public object GetValue()
      {
         return Value;
      }

      /// <summary>
      /// Gets the type of the comparison.
      /// </summary>
      /// <returns></returns>
      public ComparisonType GetComparisonType()
      {
         return Type;
      }

      /// <summary>
      /// Gets an accessor to the member that the argument should be compared with.
      /// </summary>
      /// <param name="parameter">This is the parameter representing the entity.</param>
      /// <returns></returns>
      public MemberAccess GetMemberAccess( ParameterExpression parameter )
      {
         return ExpressionHelper.CalculateMemberAccess( parameter, Path );
      }
   }
}
