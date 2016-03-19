using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public interface IPageForm
   {
      int? GetSkip();

      int? GetTake();

      int? GetPage();

      string GetOrderBy();
   }
}
