using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch.Form
{
   /// <summary>
   /// Interface representing a form that can be used to filter and pagination
   /// a set of entities.
   /// </summary>
   public interface ISearchForm : IFilterForm, IPageForm
   {
   }
}
