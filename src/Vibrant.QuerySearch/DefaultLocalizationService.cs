using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// Default implementation of ILocalizationService that does nothing.
   /// </summary>
   public class DefaultLocalizationService : ILocalizationService
   {
      /// <summary>
      /// Simply returns the provided key.
      /// </summary>
      /// <param name="entityType">The entity that is being queried  for.</param>
      /// <param name="key">The key that should be translated.</param>
      /// <returns>The provided the key.</returns>
      public string GetLocalization( Type entityType, string key )
      {
         return key;
      }
   }
}
