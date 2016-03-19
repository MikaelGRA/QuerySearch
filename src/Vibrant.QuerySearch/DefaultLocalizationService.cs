using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public class DefaultLocalizationService : ILocalizationService
   {
      public string GetLocalization( Type entityType, string key )
      {
         return key;
      }
   }
}
