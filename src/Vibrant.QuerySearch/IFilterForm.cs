using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// Interface to be implemented by forms that supports filtering.
   /// </summary>
   public interface IFilterForm
   {
      /// <summary>
      /// Gets the user provided text term.
      /// </summary>
      /// <returns>A text term that was provided by the user.</returns>
      string GetTerm();

      /// <summary>
      /// Gets or sets the way to combine the additional filters.
      /// </summary>
      /// <returns>A way to compose additional filters.</returns>
      FilterComposition GetFilterComposition();

      /// <summary>
      /// Gets any additional filters to be used for filtering.
      /// </summary>
      /// <returns>An enumerable of property comparisons.</returns>
      IEnumerable<IMemberComparison> GetAdditionalFilters();
   }
}
