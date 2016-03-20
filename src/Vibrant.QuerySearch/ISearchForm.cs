using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// Interface representing a form that can be used to filter and pagination
   /// a set of entities.
   /// </summary>
   public interface ISearchForm : IFilterForm, IPageForm
   {
   }
}
