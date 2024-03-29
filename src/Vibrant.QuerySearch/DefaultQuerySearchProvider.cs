﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Vibrant.QuerySearch.Form;
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
      private static readonly MethodInfo StartsWith = typeof( string ).GetTypeInfo().GetMethod( "StartsWith", new[] { typeof( string ) } );
      private static readonly MethodInfo Contains = typeof( string ).GetTypeInfo().GetMethod( "Contains", new[] { typeof( string ) } );

      private static readonly HashSet<string> AllowedUniqueSortMethods = new HashSet<string> { "ThenBy", "ThenByDescending" };
      private static readonly HashSet<string> AllowedDefaultSortMethods = new HashSet<string> { "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending" };

      private List<QueryOrderByInfo> _defaultSortInfo;
      private List<QueryOrderByInfo> _uniqueSortInfo;
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
         _parameter = Expression.Parameter( typeof( TEntity ), "x" );
         _uniqueSortInfo = new List<QueryOrderByInfo>();
         _defaultSortInfo = new List<QueryOrderByInfo>();

         WordCombiner = WordSearchCombiner.And;
         PageSize = 20;
         MaxTake = 20;
         MinPageSize = 1;
         MaxPageSize = 20;
         PredefinedPageSizes = new HashSet<int>();
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

      /// <summary>
      /// Gets or sets the minimum page size.
      /// </summary>
      public int MinPageSize { get; set; }

      /// <summary>
      /// Gets or sets the maximum page size.
      /// </summary>
      public int MaxPageSize { get; set; }

      /// <summary>
      /// Gets or sets whether And/Or should be used to combine word for text searches.
      /// </summary>
      public WordSearchCombiner WordCombiner { get; set; }

      /// <summary>
      /// Applies the pagination form to the query.
      /// </summary>
      /// <param name="query">The query that should be paginated.</param>
      /// <param name="form">The pagination form that should be applied to the query.</param>
      /// <returns>A pagination result.</returns>
      public virtual PaginationResult<TEntity> ApplyPagination( IQueryable<TEntity> query, IPageForm form )
      {
         var sorting = form.GetSorting( _parameter )?.ToList();
         var alreadySortedByPropertyPaths = new List<string>();

         if( sorting != null && sorting.Count > 0 )
         {
            // first order by user specified sorting

            bool isSorted = false;
            foreach( var sort in sorting )
            {
               if( sort.MemberAccessor != null )
               {
                  query = query.OrderBy( _parameter, sort, isSorted );
               }
               else
               {
                  query = ApplyManualOrdering( query, sort.PropertyPath, sort.SortDirection, isSorted );
               }
               isSorted = true;

               alreadySortedByPropertyPaths.Add( sort.PropertyPath );
            }

            query = ApplyUniqueSort( alreadySortedByPropertyPaths, (IOrderedQueryable<TEntity>)query );
         }
         else
         {
            query = ApplyDefaultSort( alreadySortedByPropertyPaths, query );
            query = ApplyUniqueSort( alreadySortedByPropertyPaths, (IOrderedQueryable<TEntity>)query );
         }

         return CreatePaginationResult( query, form, true );
      }

      protected virtual IQueryable<TEntity> ApplyManualOrdering( IQueryable<TEntity> query, string propertyPath, SortDirection direction, bool isSorted )
      {
         throw new QuerySearchException( $"Could not find the property at path '{propertyPath}' on the type '{typeof( TEntity ).FullName}'." );
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
                  throw new QuerySearchException( "No page size has been specified in the query." );
               }
               if( !PredefinedPageSizes.Contains( pageSize.Value ) )
               {
                  throw new QuerySearchException( "The specified page size is not allowed." );
               }
               return pageSize.Value;
            case PaginationMode.AnyPageSize:
               return form.GetPageSize() ?? PageSize;
            case PaginationMode.MinMaxPageSize:
               var ps = form.GetPageSize();
               if( !ps.HasValue )
               {
                  throw new QuerySearchException( "No page size has been specified in the query." );
               }
               if( ps < MinPageSize || ps > MaxPageSize )
               {
                  throw new QuerySearchException( "Invalid page size was specified." );
               }
               return ps.Value;
            default:
               throw new QuerySearchException( $"Invalid PaginationMode configured: {Mode}." );
         }
      }

      /// <summary>
      /// Registers the default sorting behaviour.
      /// </summary>
      /// <param name="applyDefaultSort">A callback to apply an OrderBy expression to a queryable.</param>
      public void RegisterDefaultSort( Expression<Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>> applyDefaultSort )
      {
         ConstructQueryOrderByInfoForDefaultSort( applyDefaultSort );
      }

      /// <summary>
      /// Registers a unique sorting behaviour.
      /// </summary>
      /// <param name="applyUniqueSort">A callback to apply a ThenBy expression to an ordered queryable.</param>
      public void RegisterUniqueSort( Expression<Func<IOrderedQueryable<TEntity>, IOrderedQueryable<TEntity>>> applyUniqueSort )
      {
         ConstructQueryOrderByInfoForUniqueSort( applyUniqueSort );
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
               predicate1 = AddExpression( predicate1, _getFreeWordPredicate( word ), WordCombiner == WordSearchCombiner.And ? ExpressionType.AndAlso : ExpressionType.OrElse );
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
         for( int i = 0; i < words.Length; i++ )
         {
            for( int j = i; j < words.Length; j++ )
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


         if( currentBody != null )
         {
            query = query.Where( currentBody );
         }


         var parameters = filteringForm.GetAdditionalFilters();
         if( parameters != null )
         {
            foreach( var propertyComparison in parameters )
            {
               var comparisonType = propertyComparison.GetComparisonType();
               var memberAccess = propertyComparison.GetMemberAccess( _parameter );
               var propertyValue = propertyComparison.GetValue();
               var memberAccessor = memberAccess.MemberAccessor;
               if( memberAccessor != null )
               {
                  var propertyType = memberAccess.MemberType;
                  var convertedPropertyValue = ConvertValue( propertyValue, propertyType );

                  Expression left = null;
                  switch( comparisonType )
                  {
                     case ComparisonType.Equal:
                        left = Expression.Equal( memberAccessor, ExpressionHelper.WrappedConstant( memberAccessor.Type, convertedPropertyValue ) );
                        break;
                     case ComparisonType.GreaterThan:
                        left = Expression.GreaterThan( memberAccessor, ExpressionHelper.WrappedConstant( memberAccessor.Type, convertedPropertyValue ) );
                        break;
                     case ComparisonType.GreaterThanOrEqual:
                        left = Expression.GreaterThanOrEqual( memberAccessor, ExpressionHelper.WrappedConstant( memberAccessor.Type, convertedPropertyValue ) );
                        break;
                     case ComparisonType.LessThan:
                        left = Expression.LessThan( memberAccessor, ExpressionHelper.WrappedConstant( memberAccessor.Type, convertedPropertyValue ) );
                        break;
                     case ComparisonType.LessThanOrEqual:
                        left = Expression.LessThanOrEqual( memberAccessor, ExpressionHelper.WrappedConstant( memberAccessor.Type, convertedPropertyValue ) );
                        break;
                     case ComparisonType.StartsWith:
                        left = Expression.Call( memberAccessor, StartsWith, ExpressionHelper.WrappedConstant( memberAccessor.Type, convertedPropertyValue ) );
                        break;
                     case ComparisonType.Contains:
                        left = Expression.Call( memberAccessor, Contains, ExpressionHelper.WrappedConstant( memberAccessor.Type, convertedPropertyValue ) );
                        break;
                     case ComparisonType.IsAnyOf:
                        if( convertedPropertyValue is IEnumerable enumerable )
                        {
                           foreach( var collectionItem in enumerable )
                           {
                              var convertedCollectionItem = ConvertValue( collectionItem, propertyType );
                              var parameterizedCollectionItem = ExpressionHelper.WrappedConstant( memberAccessor.Type, convertedCollectionItem );
                              if( left == null )
                              {
                                 left = Expression.Equal( memberAccessor, parameterizedCollectionItem );
                              }
                              else
                              {
                                 left = Expression.Or( left, Expression.Equal( memberAccessor, parameterizedCollectionItem ) );
                              }
                           }
                        }
                        else
                        {
                           throw new QuerySearchException( "Expected collection for property: " + memberAccess.PropertyPath );
                        }
                        break;
                     default:
                        throw new InvalidOperationException( $"Invalid comparison type '{comparisonType}'." );
                  }

                  var expression = Expression.Lambda<Func<TEntity, bool>>( left, _parameter );

                  query = query.Where( expression );
               }
               else
               {
                  query = ApplyManualFiltering( query, memberAccess.PropertyPath, propertyValue );
               }
            }
         }

         return query;
      }

      private static object ConvertValue( object originalValue, Type propertyType )
      {
         var unwrappedPropertyType = propertyType.GetTypeInfo().IsGenericType && propertyType.GetGenericTypeDefinition() == typeof( Nullable<> )
            ? propertyType.GetTypeInfo().GetGenericArguments()[ 0 ]
            : propertyType;

         object convertedPropertyValue;
         if( originalValue is string && ( unwrappedPropertyType == typeof( Guid ) || unwrappedPropertyType == typeof( Guid? ) ) )
         {
            convertedPropertyValue = Guid.Parse( (string)originalValue );
         }
         else if( unwrappedPropertyType.IsEnum && originalValue is string originalStringValue )
         {
            convertedPropertyValue = Enum.Parse( unwrappedPropertyType, originalStringValue, true );
         }
         else if( originalValue is IConvertible )
         {
            convertedPropertyValue = Convert.ChangeType( originalValue, unwrappedPropertyType );
         }
         else
         {
            convertedPropertyValue = originalValue;
         }

         return convertedPropertyValue;
      }

      protected virtual IQueryable<TEntity> ApplyManualFiltering( IQueryable<TEntity> query, string propertyPath, object value )
      {
         throw new QuerySearchException( $"Could not find the property at path '{propertyPath}' on the type '{typeof( TEntity ).FullName}'." );
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
         lock( _localizationToPredicate )
         {
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

      private IQueryable<TEntity> ApplyDefaultSort( List<string> alreadySortedByPropertyPaths, IQueryable<TEntity> query )
      {
         if( _defaultSortInfo.Count > 0 )
         {
            int appliedCount = 0;
            ParameterExpression queryParameter = Expression.Parameter( typeof( IQueryable<TEntity> ), "query" );
            Expression currentExpression = queryParameter;
            foreach( var info in _defaultSortInfo )
            {
               if( alreadySortedByPropertyPaths != null && info.PropertyPath != null )
               {
                  // ignore this property path, if we already sorted by it
                  if( alreadySortedByPropertyPaths.Any( propertyPath => StringComparer.OrdinalIgnoreCase.Equals( propertyPath, info.PropertyPath ) ) )
                  {
                     continue;
                  }
               }

               // make a method call on current expression
               currentExpression = Expression.Call( info.Method, currentExpression, info.LambdaExpression );
               alreadySortedByPropertyPaths.Add( info.PropertyPath );
               appliedCount++;
            }

            if( appliedCount > 0 )
            {
               var lambda = Expression.Lambda<Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>>( currentExpression, queryParameter );
               var func = lambda.Compile();
               query = func( query );
            }
         }
         return query;
      }

      private IQueryable<TEntity> ApplyUniqueSort( List<string> alreadySortedByPropertyPaths, IOrderedQueryable<TEntity> query )
      {
         if( _uniqueSortInfo.Count > 0 )
         {
            int appliedCount = 0;
            ParameterExpression queryParameter = Expression.Parameter( typeof( IOrderedQueryable<TEntity> ), "query" );
            Expression currentExpression = queryParameter;
            foreach( var info in _uniqueSortInfo )
            {
               if( alreadySortedByPropertyPaths != null && info.PropertyPath != null )
               {
                  // ignore this property path, if we already sorted by it
                  if( alreadySortedByPropertyPaths.Any( propertyPath => StringComparer.OrdinalIgnoreCase.Equals( propertyPath, info.PropertyPath ) ) )
                  {
                     continue;
                  }
               }

               // make a method call on current expression
               currentExpression = Expression.Call( info.Method, currentExpression, info.LambdaExpression );
               alreadySortedByPropertyPaths.Add( info.PropertyPath );
               appliedCount++;
            }

            if( appliedCount > 0 )
            {
               var lambda = Expression.Lambda<Func<IOrderedQueryable<TEntity>, IOrderedQueryable<TEntity>>>( currentExpression, queryParameter );
               var func = lambda.Compile();
               query = func( query );
            }
         }
         return query;
      }

      private void ConstructQueryOrderByInfoForDefaultSort( Expression<Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>> expression )
      {
         _defaultSortInfo = new List<QueryOrderByInfo>();

         var methodCallExpression = expression.Body as MethodCallExpression;
         if( methodCallExpression != null )
         {
            ConstructQueryOrderByInfo( AllowedDefaultSortMethods, _defaultSortInfo, methodCallExpression );
         }
      }

      private void ConstructQueryOrderByInfoForUniqueSort( Expression<Func<IOrderedQueryable<TEntity>, IOrderedQueryable<TEntity>>> expression )
      {
         _uniqueSortInfo = new List<QueryOrderByInfo>();

         var methodCallExpression = expression.Body as MethodCallExpression;
         if( methodCallExpression != null )
         {
            ConstructQueryOrderByInfo( AllowedUniqueSortMethods, _uniqueSortInfo, methodCallExpression );
         }
      }

      private static void ConstructQueryOrderByInfo( ISet<string> allowedMethods, List<QueryOrderByInfo> orderByInfos, MethodCallExpression methodCallExpression )
      {
         var method = methodCallExpression.Method; // expected to be ThenBy or OrderBy on Queryable
         if( allowedMethods.Contains( method.Name ) )
         {
            var previousMethodCallExpression = methodCallExpression.Arguments[ 0 ] as MethodCallExpression;
            if( previousMethodCallExpression != null )
            {
               ConstructQueryOrderByInfo( allowedMethods, orderByInfos, previousMethodCallExpression );
            }

            var expectedLambdaExpression = methodCallExpression.Arguments[ 1 ];
            var unquotedLambdaExpression = Unquote( expectedLambdaExpression );
            var lambdaExpression = unquotedLambdaExpression as LambdaExpression;
            if( lambdaExpression != null )
            {
               string propertyPath = null;
               var memberExpression = lambdaExpression.Body as MemberExpression;
               if( memberExpression != null )
               {
                  var members = new List<MemberInfo>();
                  var member = memberExpression.Member;
                  members.Add( member );
                  var parameterOrMemberExpression = memberExpression.Expression;
                  while( parameterOrMemberExpression is MemberExpression )
                  {
                     var newMemberExpression = (MemberExpression)parameterOrMemberExpression;
                     members.Insert( 0, newMemberExpression.Member );
                     parameterOrMemberExpression = newMemberExpression.Expression;
                  }

                  propertyPath = string.Join( ".", members.Select( x => x.Name ) );
               }

               orderByInfos.Add( new QueryOrderByInfo( method, expectedLambdaExpression, propertyPath ) );
            }
         }
         else
         {
            throw new InvalidOperationException( "You are only allowed to call the following methods when registering this callback: " + string.Join( ", ", allowedMethods ) );
         }
      }

      private static Expression Unquote( Expression expression )
      {
         if( expression.NodeType == ExpressionType.Quote )
         {
            return ( (UnaryExpression)expression ).Operand;
         }
         return expression;
      }

      private class QueryOrderByInfo
      {
         public QueryOrderByInfo( MethodInfo method, Expression lambdaExpression, string propertyPath )
         {
            Method = method;
            LambdaExpression = lambdaExpression;
            PropertyPath = propertyPath;
         }

         public MethodInfo Method { get; set; }

         public string PropertyPath { get; set; }

         public Expression LambdaExpression { get; set; }
      }
   }
}
