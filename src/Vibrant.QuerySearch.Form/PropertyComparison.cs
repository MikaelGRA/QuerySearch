using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch.Form
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
      /// Gets the path to compare.
      /// </summary>
      /// <returns>The path of the column to compare with.</returns>
      public string GetPath()
      {
         return Path;
      }

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
   }
}
