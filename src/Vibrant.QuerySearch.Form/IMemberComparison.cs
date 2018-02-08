using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch.Form
{
   /// <summary>
   /// An interface representing the comparison of a member to a value.
   /// </summary>
   public interface IMemberComparison
   {
      /// <summary>
      /// Gets the path to compare.
      /// </summary>
      /// <returns>The path of the column to compare with.</returns>
      string GetPath();

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
   }
}
