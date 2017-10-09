using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch.EntityFrameworkCore
{
   public enum FtsSearchMode
   {
      FreeText,
      WeightedPrefixes,
      WeightedPrefixesPlusReverse
   }
}
