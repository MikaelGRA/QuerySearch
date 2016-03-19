﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// This is the type of a comparison that can be performed on a IQueryable.
   /// </summary>
   public enum ComparisonType
   {
      /// <summary>
      /// An equals comparison (==)
      /// </summary>
      Equal = 1,

      /// <summary>
      /// A greater than comparison (>)
      /// </summary>
      GreaterThan = 2,

      /// <summary>
      /// A greater than or equals comparison (>=)
      /// </summary>
      GreaterThanOrEqual = 3,

      /// <summary>
      /// A less than comparison (<)
      /// </summary>
      LessThan = 4,

      /// <summary>
      /// A less than or equals comparison (>=)
      /// </summary>
      LessThanOrEqual = 5,
   }
}
