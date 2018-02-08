using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch.Form
{
   /// <summary>
   /// Default implementation of a SearchForm that has full searching capabilities.
   /// </summary>
   public class SearchForm : ISearchForm
   {
      /// <summary>
      /// Gets or sets the number of entries to skip.
      /// </summary>
      public int? Skip { get; set; }

      /// <summary>
      /// Gets or sets the number of entries to take.
      /// </summary>
      public int? Take { get; set; }

      /// <summary>
      /// Gets or sets the page to retrieve.
      /// </summary>
      public int? Page { get; set; }

      /// <summary>
      /// Gets or sets the size of the pages to retrieve.
      /// </summary>
      public int? PageSize { get; set; }

      /// <summary>
      /// Gets or sets a string representing the ordering of the retruend entries.
      /// </summary>
      public string OrderBy { get; set; }

      /// <summary>
      /// Gets or sets a bool indicating if results should be ranked by the term.
      /// </summary>
      public bool OrderByTerm { get; set; }

      /// <summary>
      /// Gets or sets the user provided search term.
      /// </summary>
      public string Term { get; set; }

      /// <summary>
      /// Gets or sets any additional search parameters.
      /// </summary>
      public List<PropertyComparison> Parameters { get; set; }

      /// <summary>
      /// Gets the user provided text term.
      /// </summary>
      /// <returns>A text term that was provided by the user.</returns>
      public string GetTerm()
      {
         return Term;
      }

      /// <summary>
      /// Gets any additional filters to be used for filtering.
      /// </summary>
      /// <returns>An enumerable of property comparisons.</returns>
      public IEnumerable<IMemberComparison> GetAdditionalFilters()
      {
         return Parameters;
      }

      /// <summary>
      /// Gets the number of items to skip.
      /// </summary>
      /// <returns></returns>
      public int? GetSkip()
      {
         return Skip;
      }

      /// <summary>
      /// Gets the number of items to take.
      /// </summary>
      /// <returns></returns>
      public int? GetTake()
      {
         return Take;
      }

      /// <summary>
      /// Gets the page number.
      /// </summary>
      /// <returns></returns>
      public int? GetPage()
      {
         return Page;
      }

      /// <summary>
      /// Gets the size of the pages to return.
      /// </summary>
      /// <returns></returns>
      public int? GetPageSize()
      {
         return PageSize;
      }

      /// <summary>
      /// Gets or sets a bool indicating if results should be ranked by the term.
      /// </summary>
      public bool SortByTermRank()
      {
         return OrderByTerm;
      }

      /// <summary>
      /// Gets a string indicate how the result should be ordered.
      /// </summary>
      /// <returns>An order string.</returns>
      public string GetOrderBy()
      {
         return OrderBy;
      }
   }
}
