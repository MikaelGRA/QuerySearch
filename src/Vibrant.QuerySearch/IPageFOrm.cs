using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
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
      /// Gets the sorting to be used.
      /// </summary>
      /// <returns></returns>
      IEnumerable<SortMemberAccess> GetSorting( ParameterExpression parameter );
   }
}
