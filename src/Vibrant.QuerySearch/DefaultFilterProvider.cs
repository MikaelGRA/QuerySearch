using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Text;
using Vibrant.QuerySearch.Helpers;

namespace Vibrant.QuerySearch
{
   /// <summary>
   /// A default implrmentation of IFilterProvider that provides support for:
   /// <list type="bullet">
   /// <item>
   /// <description>Sentence search.</description>
   /// </item>
   /// <item>
   /// <description>Word search.</description>
   /// </item>
   /// <item>
   /// <description>Keyword search.</description>
   /// </item>
   /// <item>
   /// <description>Localized keyword search.</description>
   /// </item>
   /// </list>
   /// </summary>
   /// <typeparam name="TEntity"></typeparam>
   public class DefaultFilterProvider<TEntity> : IFilterProvider<TEntity>
   {
      private Dictionary<string, Expression<Func<TEntity, bool>>> _predefinedPredicates;
      private Func<string, Expression<Func<TEntity, bool>>> _getFreeWordPredicate;
      private Func<string, Expression<Func<TEntity, bool>>> _getFreeSentencePredicate;
      private Dictionary<CultureInfo, Dictionary<string, Expression<Func<TEntity, bool>>>> _localizationToPredicate;
      private Dictionary<string, Expression<Func<TEntity, bool>>> _localizationKeyToPredicate;
      private ParameterExpression _parameter;

      private ILocalizationService _localizationService;

      /// <summary>
      /// Constructs a DefaultFilterProvider.
      /// </summary>
      /// <param name="localization"></param>
      public DefaultFilterProvider( ILocalizationService localization )
      {
         _localizationService = localization;
         _predefinedPredicates = new Dictionary<string, Expression<Func<TEntity, bool>>>( StringComparer.OrdinalIgnoreCase );
         _localizationToPredicate = new Dictionary<CultureInfo, Dictionary<string, Expression<Func<TEntity, bool>>>>();
         _localizationKeyToPredicate = new Dictionary<string, Expression<Func<TEntity, bool>>>();
         _parameter = Expression.Parameter( typeof( TEntity ), "x" );
      }

      /// <summary>
      /// Applies filtering to the IQueryable provided.
      /// </summary>
      /// <param name="query">The IQueryable that is to be filtered.</param>
      /// <param name="filteringForm">The filtering form.</param>
      /// <returns>A filtered IQueryable.</returns>
      public IQueryable<TEntity> ApplyWhere( IQueryable<TEntity> query, IFilterForm filteringForm )
      {
         var term = filteringForm.GetTerm();
         var words = GetWords( term );

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
         Expression<Func<TEntity, bool>> currentBody = null;
         if( predicate1 != null && predicate2 != null )
         {
            currentBody = predicate1.Or( predicate2 );
         }
         else if( predicate1 != null )
         {
            currentBody = predicate1;
         }
         else if( predicate2 != null )
         {
            currentBody = predicate2;
         }

         // All of the remaining filters are required
         var localizations = GetLocalizationDictionary();
         foreach( var word in words )
         {
            Expression<Func<TEntity, bool>> predicate;
            if( _predefinedPredicates.TryGetValue( word, out predicate ) )
            {
               currentBody = AddExpression( currentBody, predicate, ExpressionType.AndAlso );
            }

            Expression<Func<TEntity, bool>> localizedPredicate;
            if( localizations.TryGetValue( word, out localizedPredicate ) )
            {
               currentBody = AddExpression( currentBody, localizedPredicate, ExpressionType.AndAlso );
            }
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

      private Dictionary<string, Expression<Func<TEntity, bool>>> GetLocalizationDictionary()
      {
         var culture = CultureInfo.CurrentCulture;

         Dictionary<string, Expression<Func<TEntity, bool>>> localizations;
         if( !_localizationToPredicate.TryGetValue( culture, out localizations ) )
         {
            localizations = new Dictionary<string, Expression<Func<TEntity, bool>>>();
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