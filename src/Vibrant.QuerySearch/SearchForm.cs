using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public class SearchForm : ISearchForm
   {
      public int? Skip { get; set; }

      public int? Take { get; set; }

      public int? Page { get; set; }

      public string OrderBy { get; set; }

      public string Term { get; set; }

      public List<PropertyComparison> Parameters { get; set; }

      public string GetTerm()
      {
         return Term;
      }

      public IEnumerable<IMemberComparison> GetAdditionalFilters()
      {
         return Parameters;
      }

      public int? GetSkip()
      {
         return Skip;
      }

      public int? GetTake()
      {
         return Take;
      }

      public int? GetPage()
      {
         return Page;
      }

      public IEnumerable<SortMemberAccess> GetSorting( ParameterExpression parameter )
      {
         if( !string.IsNullOrWhiteSpace( Term ) )
         {
            return ExpressionHelper.CalculateSortMemberAccess( parameter, OrderBy );
         }
         return null;
      }
   }
}
