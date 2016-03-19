using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Text;
using Vibrant.QuerySearch.Predicates;

namespace Vibrant.QuerySearch
{
   public class DefaultFilterProvider<TEntity> : IFilterProvider<TEntity>
   {
      private Dictionary<string, Expression<Func<TEntity, bool>>> _predefinedPredicates;
      private Func<string, Expression<Func<TEntity, bool>>> _getFreeWordPredicate;
      private Func<string, Expression<Func<TEntity, bool>>> _getFreeSentencePredicate;
      private Dictionary<CultureInfo, Dictionary<string, Expression<Func<TEntity, bool>>>> _localizationToPredicate;
      private Dictionary<string, Expression<Func<TEntity, bool>>> _localizationKeyToPredicate;
      private ParameterExpression _parameter;

      private ILocalizationService _localizationService;

      public DefaultFilterProvider( ILocalizationService localization )
      {
         _localizationService = localization;
         _predefinedPredicates = new Dictionary<string, Expression<Func<TEntity, bool>>>( StringComparer.OrdinalIgnoreCase );
         _localizationToPredicate = new Dictionary<CultureInfo, Dictionary<string, Expression<Func<TEntity, bool>>>>();
         _localizationKeyToPredicate = new Dictionary<string, Expression<Func<TEntity, bool>>>();
         _parameter = Expression.Parameter( typeof( TEntity ), "x" );
      }

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
               var propertyGetter = propertyComparison.GetPropertyGetter( _parameter );
               var propertyValue = Convert.ChangeType( propertyComparison.GetValue(), propertyComparison.GetValueType( _parameter ) );
               var constant = Expression.Constant( propertyValue );

               Expression left = null;
               switch( comparisonType )
               {
                  case ComparisonType.Equal:
                     left = Expression.Equal( propertyGetter, constant );
                     break;
                  case ComparisonType.GreaterThan:
                     left = Expression.GreaterThan( propertyGetter, constant );
                     break;
                  case ComparisonType.GreaterThanOrEqual:
                     left = Expression.GreaterThanOrEqual( propertyGetter, constant );
                     break;
                  case ComparisonType.LessThan:
                     left = Expression.LessThan( propertyGetter, constant );
                     break;
                  case ComparisonType.LessThanOrEqual:
                     left = Expression.LessThanOrEqual( propertyGetter, constant );
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
      public void RegisterKeyword( string keyword, Expression<Func<TEntity, bool>> predicate )
      {
         _predefinedPredicates[ keyword ] = predicate;
      }

      public void RegisterLocalizedKeyword( string key, Expression<Func<TEntity, bool>> predicate )
      {
         _localizationKeyToPredicate[ key ] = predicate;
      }

      public void RegisterWordSearch( Func<string, Expression<Func<TEntity, bool>>> getPredicate )
      {
         _getFreeWordPredicate = getPredicate;
      }

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