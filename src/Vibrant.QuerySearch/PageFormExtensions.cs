using System.Collections.Generic;
using System.Linq.Expressions;
using Vibrant.QuerySearch.Form;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// Extension methods for IPageForm.
   /// </summary>
   public static class PageFormExtensions
   {
      /// <summary>
      /// Gets the sorting to be used.
      /// </summary>
      /// <returns></returns>
      public static IEnumerable<SortMemberAccess> GetSorting( this IPageForm form, ParameterExpression parameter )
      {
         if( !string.IsNullOrWhiteSpace( form.GetOrderBy() ) )
         {
            return ExpressionHelper.CalculateSortMemberAccesses( parameter, form.GetOrderBy() );
         }
         return null;
      }
   }
}
