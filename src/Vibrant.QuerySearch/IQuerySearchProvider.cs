using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.QuerySearch
{
    public interface IQuerySearchProvider<TEntity> : IFilterProvider<TEntity>, IPaginationProvider<TEntity>
    {
    }
}
