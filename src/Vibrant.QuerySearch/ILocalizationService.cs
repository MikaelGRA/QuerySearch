using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public interface ILocalizationService
   {
      string GetLocalization( Type entityType, string key );
   }
}
