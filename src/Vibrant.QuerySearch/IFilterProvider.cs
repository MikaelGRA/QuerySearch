using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
   public interface IFilterProvider<TEntity>
   {
      IQueryable<TEntity> ApplyWhere( IQueryable<TEntity> query, IFilterForm form );
   }
}
