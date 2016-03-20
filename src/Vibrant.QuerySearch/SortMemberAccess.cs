using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// Utility class used to represent the accessing of a member and its type with a sort direction.
   /// </summary>
   public class SortMemberAccess : MemberAccess
   {
      public SortMemberAccess( Type memberType, SortDirection sortDirection, Expression memberAccessor )
         : base( memberType, memberAccessor )
      {
         SortDirection = sortDirection;
      }

      /// <summary>
      /// Gets the sort direction.
      /// </summary>
      public SortDirection SortDirection { get; private set; }
   }
}
