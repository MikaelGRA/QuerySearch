using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vibrant.QuerySearch.Form;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// Extension methods for IMemberComparison.
   /// </summary>
   public static class MemberComparisonExtensions
   {
      /// <summary>
      /// Gets an accessor to the member that the argument should be compared with.
      /// </summary>
      /// <param name="comparison"></param>
      /// <param name="parameter">This is the parameter representing the entity.</param>
      /// <returns></returns>
      public static MemberAccess GetMemberAccess( this IMemberComparison comparison, ParameterExpression parameter )
      {
         return ExpressionHelper.CalculateMemberAccess( parameter, comparison.GetPath() );
      }
   }
}
