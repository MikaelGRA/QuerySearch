using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   internal class PropertyPathResult
   {
      internal PropertyPathResult( Expression getter, Type propertyType )
      {
         PropertyGetter = getter;
         PropertyType = propertyType;
      }

      internal Expression PropertyGetter { get; private set; }

      internal Type PropertyType { get; private set; }
   }
}
