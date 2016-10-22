using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Vibrant.QuerySearch.Helpers;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// An implenentation of IQuerySearchProvider which implements
   /// both the IPaginationProvider and IFilterProvider interfaces.
   /// </summary>
   /// <typeparam name="TEntity"></typeparam>
   public class DefaultQuerySearchProvider<TEntity> : IQuerySearchProvider<TEntity>
   {
      private dynamic _defaultSort;
      private SortDirection _defaultSortDirection;
      private dynamic _uniqueSort;
      private SortDirection _uniqueSortDirection;
      private ParameterExpression _parameter;

      private Dictionary<string, Expression<Func<TEntity, bool>>> _predefinedPredicates;
      private Func<string, Expression<Func<TEntity, bool>>> _getFreeWordPredicate;
      private Func<string, Expression<Func<TEntity, bool>>> _getFreeSentencePredicate;
      private Dictionary<CultureInfo, Dictionary<string, Expression<Func<TEntity, bool>>>> _localizationToPredicate;
      private Dictionary<string, Expression<Func<TEntity, bool>>> _localizationKeyToPredicate;
      private Func<string, Func<IQueryable<TEntity>, IQueryable<TEntity>>> _getApplyWhere;

      private ILocalizationService _localizationService;

      /// <summary>
      /// Constructs an PagedPaginationProvider with a page size of 20.
      /// </summary>
      public DefaultQuerySearchProvider( ILocalizationService localization )
      {
         _localizationService = localization;
         _predefinedPredicates = new Dictionary<string, Expression<Func<TEntity, bool>>>( StringComparer.OrdinalIgnoreCase );
         _localizationToPredicate = new Dictionary<CultureInfo, Dictionary<string, Expression<Func<TEntity, bool>>>>();
         _localizationKeyToPredicate = new Dictionary<string, Expression<Func<TEntity, bool>>>();

         PageSize = 20;
         MaxTake = 20;
         PredefinedPageSizes = new HashSet<int>();
         _parameter = Expression.Parameter( typeof( TEntity ), "x" );
      }

      /// <summary>
      /// Gets or sets the PageSize.
      /// </summary>
      public int PageSize { get; set; }

      /// <summary>
      /// Gets or sets the pagination mode.
      /// </summary>
      public PaginationMode Mode { get; set; }

      /// <summary>
      /// Gets the predined page sizes.
      /// </summary>
      public ICollection<int> PredefinedPageSizes { get; private set; }

      /// <summary>
      /// Gets or sets the MaxTake.
      /// </summary>
      public int MaxTake { get; set; }

      private IOrderedQueryable<TEntity> ApplyOrdering(
         IQueryable<TEntity> query,
         dynamic sort,
         SortDirection direction,
         ref bool isSorted )
      {
         if( isSorted )
         {
            if( direction == SortDirection.Ascending )
            {
               query = Queryable.ThenBy( (IOrderedQueryable<TEntity>)query, sort );
            }
            else
            {
               query = Queryable.ThenByDescending( (IOrderedQueryable<TEntity>)query, sort );
            }
         }
         else
         {
            if( direction == SortDirection.Ascending )
            {
               query = Queryable.OrderBy( query, sort );
            }
            else
            {
               query = Queryable.OrderByDescending( query, sort );
            }
         }

         isSorted = true;

         return (IOrderedQueryable<TEntity>)query;
      }

      /// <summary>
      /// Applies the pagination form to the query.
      /// </summary>
      /// <param name="query">The query that should be paginated.</param>
      /// <param name="form">The pagination form that should be applied to the query.</param>
      /// <returns>A pagination result.</returns>
      public virtual PaginationResult<TEntity> ApplyPagination( IQueryable<TEntity> query, IPageForm form )
      {
         bool isSorted = false;

         var sorting = form.GetSorting( _parameter )?.ToList();
         if( sorting != null && sorting.Count > 0 )
         {
            // first order by user specified sorting
            query = query.OrderBy( _parameter, sorting );
            isSorted = true;

            if( _uniqueSort != null )
            {
               query = ApplyOrdering( query, _uniqueSort, _uniqueSortDirection, ref isSorted );
            }
            else if( _defaultSort != null )
            {
               query = ApplyOrdering( query, _defaultSort, _defaultSortDirection, ref isSorted );
            }
         }
         else
         {
            // first order by default sorting
            if( _defaultSort != null )
            {
               query = ApplyOrdering( query, _defaultSort, _defaultSortDirection, ref isSorted );
            }

            // then order by unique sorting, if present, and not equal to unique sorting
            if( _uniqueSort != null )
            {
               if( !( ReferenceEquals( _uniqueSort, _defaultSort ) && _uniqueSortDirection == _defaultSortDirection ) )
               {
                  query = ApplyOrdering( query, _uniqueSort, _uniqueSortDirection, ref isSorted );
               }
            }
         }

         return CreatePaginationResult( query, form, true );
      }

      protected PaginationResult<TEntity> CreatePaginationResult( IQueryable<TEntity> query, IPageForm form, bool applyPagination )
      {
         var page = form.GetPage();
         var skip = form.GetSkip();
         var take = form.GetTake();

         if( Mode == PaginationMode.SkipAndTake )
         {
            int actualSkip = 0;
            if( skip.HasValue )
            {
               actualSkip = skip.Value;
            }

            int actualTake = MaxTake;
            if( take.HasValue && take.Value < MaxTake )
            {
               actualTake = MaxTake;
            }

            return new PaginationResult<TEntity>(
               actualSkip,
               actualTake,
               applyPagination ? query.Skip( actualSkip ).Take( actualTake ) : query );
         }
         else
         {
            var pageSize = GetPageSize( form );
            int actualPage = 0;
            if( page.HasValue )
            {
               actualPage = page.Value;
            }
            else if( skip.HasValue )
            {
               actualPage = skip.Value / pageSize;
            }

            return new PaginationResult<TEntity>(
               actualPage * pageSize,
               pageSize,
               actualPage,
               pageSize,
               applyPagination ? query.Skip( actualPage * pageSize ).Take( pageSize ) : query );
         }
      }

      protected int GetPageSize( IPageForm form )
      {
         switch( Mode )
         {
            case PaginationMode.PageSize:
               return PageSize;
            case PaginationMode.PredefinedPageSizes:
               var pageSize = form.GetPageSize();
               if( !pageSize.HasValue )
               {
                  throw new QuerySearchException( "No page sizes has been specified in the query." );
               }
               if( !PredefinedPageSizes.Contains( pageSize.Value ) )
               {
                  throw new QuerySearchException( "The specified page size is not allowed." );
               }
               return pageSize.Value;
            case PaginationMode.AnyPageSize:
               return form.GetPageSize() ?? PageSize;
            default:
               throw new QuerySearchException( $"Invalid PaginationMode configured: {Mode}." );
         }
      }

      /// <summary>
      /// Registers the default sorting behaviour.
      /// </summary>
      /// <typeparam name="TKey"></typeparam>
      /// <param name="defaultSort">An expression representing the default sorting.</param>
      /// <param name="direction">The direction of the default sorting.</param>
      public void RegisterDefaultSort<TKey>( Expression<Func<TEntity, TKey>> defaultSort, SortDirection direction = SortDirection.Ascending, bool isAlsoUniqueSort = true )
      {
         _defaultSort = defaultSort;
         _defaultSortDirection = direction;

         if( isAlsoUniqueSort )
         {
            _uniqueSort = defaultSort;
            _uniqueSortDirection = direction;
         }
      }

      /// <summary>
      /// Registers a unique sorting behaviour.
      /// </summary>
      /// <typeparam name="TKey"></typeparam>
      /// <param name="uniqueSort">An expression representing the unique sorting.</param>
      /// <param name="direction">The direction of the default sorting.</param>
      public void RegisterUniqueSort<TKey>( Expression<Func<TEntity, TKey>> uniqueSort, SortDirection direction = SortDirection.Ascending )
      {
         _uniqueSort = uniqueSort;
         _uniqueSortDirection = direction;
      }

      /// <summary>
      /// Applies filtering to the IQueryable provided.
      /// </summary>
      /// <param name="query">The IQueryable that is to be filtered.</param>
      /// <param name="filteringForm">The filtering form.</param>
      /// <returns>A filtered IQueryable.</returns>
      public virtual IQueryable<TEntity> ApplyWhere( IQueryable<TEntity> query, IFilterForm filteringForm )
      {
         var term = filteringForm.GetTerm();
         var words = GetWords( term );

         if( _getApplyWhere != null && term != null )
         {
            var applyWhere = _getApplyWhere( term );
            query = applyWhere( query );
         }

         // create predicate1 (word expressions)
         Expression<Func<TEntity, bool>> predicate1 = null;
         if( _getFreeWordPredicate != null )
         {
            foreach( var word in words )
            {
               predicate1 = AddExpression( predicate1, _getFreeWordPredicate( word ), ExpressionType.OrElse );
            }
         }

         // create predicate2 (sentence expression)
         Expression<Func<TEntity, bool>> predicate2 = null;
         if( _getFreeSentencePredicate != null && term != null )
         {
            predicate2 = _getFreeSentencePredicate( term );
         }

         // create predicate1 OR predicate2 as currentBody
         Expression<Func<TEntity, bool>> textPredicate = null;
         if( predicate1 != null && predicate2 != null )
         {
            textPredicate = predicate1.Or( predicate2 );
         }
         else if( predicate1 != null )
         {
            textPredicate = predicate1;
         }
         else if( predicate2 != null )
         {
            textPredicate = predicate2;
         }

         var localizations = GetLocalizationDictionary();
         Expression<Func<TEntity, bool>> keywordPredicate = null;
         for( int i = 0 ; i < words.Length ; i++ )
         {
            for( int j = i ; j < words.Length ; j++ )
            {
               // from i to j
               var word = string.Join( " ", words, i, j - i + 1 );

               Expression<Func<TEntity, bool>> predicate;
               if( _predefinedPredicates.TryGetValue( word, out predicate ) )
               {
                  keywordPredicate = AddExpression( keywordPredicate, predicate, ExpressionType.AndAlso );
               }

               Expression<Func<TEntity, bool>> localizedPredicate;
               if( localizations.TryGetValue( word, out localizedPredicate ) )
               {
                  keywordPredicate = AddExpression( keywordPredicate, localizedPredicate, ExpressionType.AndAlso );
               }
            }
         }

         // create textPredicate OR keywordPredicate as currentBody
         Expression<Func<TEntity, bool>> currentBody = null;
         if( textPredicate != null && keywordPredicate != null )
         {
            currentBody = textPredicate.Or( keywordPredicate );
         }
         else if( textPredicate != null )
         {
            currentBody = textPredicate;
         }
         else if( keywordPredicate != null )
         {
            currentBody = keywordPredicate;
         }

         var parameters = filteringForm.GetAdditionalFilters();
         if( parameters != null )
         {
            foreach( var propertyComparison in parameters )
            {
               var comparisonType = propertyComparison.GetComparisonType();
               var memberAccess = propertyComparison.GetMemberAccess( _parameter );
               var memberAccessor = memberAccess.MemberAccessor;
               var propertyType = memberAccess.MemberType;
               var unwrappedPropertyType = propertyType.GetTypeInfo().IsGenericType && propertyType.GetGenericTypeDefinition() == typeof( Nullable<> )
                  ? propertyType.GetGenericArguments()[ 0 ]
                  : propertyType;
               var propertyValue = propertyComparison.GetValue();
               var convertedPropertyValue = Convert.ChangeType( propertyValue, unwrappedPropertyType );
               var parameterizedPropertyValue = ExpressionHelper.WrappedConstant( propertyType, convertedPropertyValue );

               Expression left = null;
               switch( comparisonType )
               {
                  case ComparisonType.Equal:
                     left = Expression.Equal( memberAccessor, parameterizedPropertyValue );
                     break;
                  case ComparisonType.GreaterThan:
                     left = Expression.GreaterThan( memberAccessor, parameterizedPropertyValue );
                     break;
                  case ComparisonType.GreaterThanOrEqual:
                     left = Expression.GreaterThanOrEqual( memberAccessor, parameterizedPropertyValue );
                     break;
                  case ComparisonType.LessThan:
                     left = Expression.LessThan( memberAccessor, parameterizedPropertyValue );
                     break;
                  case ComparisonType.LessThanOrEqual:
                     left = Expression.LessThanOrEqual( memberAccessor, parameterizedPropertyValue );
                     break;
                  default:
                     throw new InvalidOperationException( $"Invalid comparison type '{comparisonType}'." );
               }

               currentBody = AddExpression( currentBody, Expression.Lambda<Func<TEntity, bool>>( left, _parameter ), ExpressionType.AndAlso );
            }
         }

         if( currentBody != null )
         {
            return query.Where( currentBody );
         }
         else
         {
            return query;
         }
      }

      /// <summary>
      /// Registers a keyword to a predicate that will be used when the keyword is
      /// encountered.
      /// </summary>
      /// <param name="keyword">The keyword to look for.</param>
      /// <param name="predicate">The predicate to be applied to an IQueryable when the keyword is encountered.</param>
      public void RegisterKeyword( string keyword, Expression<Func<TEntity, bool>> predicate )
      {
         _predefinedPredicates[ keyword ] = predicate;
      }

      /// <summary>
      /// Registers a localized keyword to a predicated that will be used when the 
      /// keyword is encountered.
      /// </summary>
      /// <param name="key">This is the key to be used to lookup the localized keyword.</param>
      /// <param name="predicate">This is the predicate that is to be applied when the keyword is encountered.</param>
      public void RegisterLocalizedKeyword( string key, Expression<Func<TEntity, bool>> predicate )
      {
         _localizationKeyToPredicate[ key ] = predicate;
      }

      /// <summary>
      /// Registers a function that will be used to apply word search to an entity.
      /// </summary>
      /// <param name="getPredicate">This is a function that returns a predicate that filters on the provided text.</param>
      public void RegisterWordSearch( Func<string, Expression<Func<TEntity, bool>>> getPredicate )
      {
         _getFreeWordPredicate = getPredicate;
      }

      /// <summary>
      /// Registers a function that will be used to apply sentence search to an entity.
      /// </summary>
      /// <param name="getPredicate">This is a function that returns a predicate that filters on the provided text.</param>
      public void RegisterSentenceSearch( Func<string, Expression<Func<TEntity, bool>>> getPredicate )
      {
         _getFreeSentencePredicate = getPredicate;
      }

      /// <summary>
      /// Registers an 'ApplyWhere' functions that will be executed on all queries.
      /// </summary>
      /// <param name="getApplyWhere">This is a function that executes a filtering function.</param>
      public void RegisterApplyWhere( Func<string, Func<IQueryable<TEntity>, IQueryable<TEntity>>> getApplyWhere )
      {
         _getApplyWhere = getApplyWhere;
      }

      private Dictionary<string, Expression<Func<TEntity, bool>>> GetLocalizationDictionary()
      {
         var culture = CultureInfo.CurrentCulture;

         Dictionary<string, Expression<Func<TEntity, bool>>> localizations;
         if( !_localizationToPredicate.TryGetValue( culture, out localizations ) )
         {
            localizations = new Dictionary<string, Expression<Func<TEntity, bool>>>( StringComparer.OrdinalIgnoreCase );
            foreach( var kvp in _localizationKeyToPredicate )
            {
               var key = kvp.Key;
               var expression = kvp.Value;
               var localization = _localizationService.GetLocalization( typeof( TEntity ), key );
               localizations.Add( localization, expression );
            }
            _localizationToPredicate.Add( culture, localizations );
         }

         return localizations;
      }

      private Expression<Func<TEntity, bool>> AddExpression( Expression<Func<TEntity, bool>> currentBody, Expression<Func<TEntity, bool>> addition, ExpressionType type )
      {
         if( currentBody == null )
         {
            return addition;
         }

         switch( type )
         {
            case ExpressionType.OrElse:
               return currentBody.Or( addition );
            case ExpressionType.AndAlso:
               return currentBody.And( addition );
            default:
               throw new InvalidOperationException();
         }
      }

      private Expression<Func<TEntity, bool>> GiveExpressionByLocation( string word )
      {
         return _predefinedPredicates[ word ];
      }

      private string[] GetWords( string term )
      {
         if( term == null )
         {
            return new string[ 0 ];
         }
         return term.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
      }
   }
}
