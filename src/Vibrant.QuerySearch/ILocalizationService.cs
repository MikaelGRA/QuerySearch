using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// An ILocalizationProvider allows using localized text when the user is searching.
   /// </summary>
   public interface ILocalizationService
   {
      /// <summary>
      /// Gets the localized value for a specified key.
      /// </summary>
      /// <param name="entityType">The entity type.</param>
      /// <param name="key">The registered keyword.</param>
      /// <returns>A localized text.</returns>
      string GetLocalization( Type entityType, string key );
   }
}
