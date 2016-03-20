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
   /// An interface representing the comparison of a member to a value.
   /// </summary>
   public interface IMemberComparison
   {
      /// <summary>
      /// Gets the argument to compare with member with.
      /// </summary>
      /// <returns>The argument to compare with member with.</returns>
      object GetValue();

      /// <summary>
      /// Gets the type of the comparison.
      /// </summary>
      /// <returns></returns>
      ComparisonType GetComparisonType();
      
      /// <summary>
      /// Gets an accessor to the member that the argument should be compared with.
      /// </summary>
      /// <param name="parameter">This is the parameter representing the entity.</param>
      /// <returns></returns>
      MemberAccess GetMemberAccess( ParameterExpression parameter );
   }
}
