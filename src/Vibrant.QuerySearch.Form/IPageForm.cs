using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch.Form
{
   /// <summary>
   /// Interface to be implemented by forms that supports pagination.
   /// </summary>
   public interface IPageForm
   {
      /// <summary>
      /// Gets the number of items to skip.
      /// </summary>
      /// <returns></returns>
      int? GetSkip();

      /// <summary>
      /// Gets the number of items to take.
      /// </summary>
      /// <returns></returns>
      int? GetTake();

      /// <summary>
      /// Gets the page number.
      /// </summary>
      /// <returns></returns>
      int? GetPage();

      /// <summary>
      /// Gets the user-defined page size.
      /// </summary>
      /// <returns></returns>
      int? GetPageSize();

      /// <summary>
      /// Gets a bool indicating if term rank should determine result ordering.
      /// </summary>
      /// <returns></returns>
      bool SortByTermRank();

      /// <summary>
      /// Gets a string that indicates how the result should be ordered.
      /// </summary>
      /// <returns></returns>
      string GetOrderBy();
   }
}
