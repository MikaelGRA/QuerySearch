using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public class MemberAccess
   {
      public MemberAccess( Type memberType, Expression memberAccessor )
      {
         MemberType = memberType;
         MemberAccessor = memberAccessor;
      }

      public Type MemberType { get; private set; }

      public Expression MemberAccessor { get; private set; }

      public SortMemberAccess WithSorting( SortDirection sort )
      {
         return new SortMemberAccess( MemberType, sort, MemberAccessor );
      }
   }
}
