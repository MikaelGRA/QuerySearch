using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vibrant.QuerySearch.EntityFrameworkCore;

namespace Vibrant.QuerySearch.ConsoleApp
{
   public class MyClassQuerySearchProvider : FtsQuerySearchProvider<MyClass>
   {
      public MyClassQuerySearchProvider(ILocalizationService localization) : base(localization)
      {
         RegisterDefaultSort(q => q.OrderBy(x => x.Id));
         RegisterUniqueSort(q => q.ThenBy(x => x.Id));
         Mode = PaginationMode.AnyPageSize;
         SearchMode = FtsSearchMode.WeightedPrefixes;
      }

      protected override string GetKeyColumnName()
      {
         return "[Id]";
      }

      protected override string GetTableName()
      {
         return "[MyClasses]";
      }

      protected override string GetUniqueColumnSort()
      {
         return "[Id] ASC";
      }
   }
}
