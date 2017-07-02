using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// Utility class used to represent the accessing of a member and its type.
   /// </summary>
   public class MemberAccess
   {
      public MemberAccess( Type memberType, Expression memberAccessor, string propertyPath )
      {
         MemberType = memberType;
         MemberAccessor = memberAccessor;
         PropertyPath = propertyPath;
      }

      /// <summary>
      /// Gets the type of the result of the MemberAccessor expression.
      /// </summary>
      public Type MemberType { get; private set; }

      /// <summary>
      /// Gets the MemberAccessor expression.
      /// </summary>
      public Expression MemberAccessor { get; private set; }

      /// <summary>
      /// Gets the property path of the member accesses.
      /// </summary>
      public string PropertyPath { get; private set; }

      /// <summary>
      /// Gets a SortMemberAccess that is sorted.
      /// </summary>
      /// <param name="sort"></param>
      /// <returns></returns>
      public SortMemberAccess WithSorting( SortDirection sort )
      {
         return new SortMemberAccess( MemberType, sort, MemberAccessor, PropertyPath );
      }
   }
}
