using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public class SortMemberAccess : MemberAccess
   {
      public SortMemberAccess( Type memberType, SortDirection sortDirection, Expression memberAccessor )
         : base( memberType, memberAccessor )
      {
         SortDirection = sortDirection;
      }

      public SortDirection SortDirection { get; private set; }
   }
}
