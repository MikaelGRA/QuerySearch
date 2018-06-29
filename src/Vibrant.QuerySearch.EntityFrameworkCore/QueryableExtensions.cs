using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Parsing.Structure;

namespace Vibrant.QuerySearch.EntityFrameworkCore
{
   public static class QueryableExtensions
   {
      //private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();
      //private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");
      //private static readonly PropertyInfo NodeTypeProviderField = QueryCompilerTypeInfo.DeclaredProperties.Single(x => x.Name == "NodeTypeProvider");
      //private static readonly MethodInfo CreateQueryParserMethod = QueryCompilerTypeInfo.DeclaredMethods.First(x => x.Name == "CreateQueryParser");
      //private static readonly FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");
      //private static readonly PropertyInfo DatabaseDependenciesProperty = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

      private static readonly FieldInfo _queryCompilerField = typeof( EntityQueryProvider ).GetTypeInfo().DeclaredFields.Single( x => x.Name == "_queryCompiler" );
      private static readonly TypeInfo _queryCompilerTypeInfo = typeof( QueryCompiler ).GetTypeInfo();
      private static readonly FieldInfo _queryModelGeneratorField = _queryCompilerTypeInfo.DeclaredFields.Single( x => x.Name == "_queryModelGenerator" );
      private static readonly FieldInfo _databaseField = _queryCompilerTypeInfo.DeclaredFields.Single( x => x.Name == "_database" );
      private static readonly PropertyInfo _dependenciesProperty = typeof( Database ).GetTypeInfo().DeclaredProperties.Single( x => x.Name == "Dependencies" );

      public static string ToSql<TEntity>( this IQueryable<TEntity> query ) where TEntity : class
      {
         if( !( query is EntityQueryable<TEntity> ) && !( query is InternalDbSet<TEntity> ) )
            throw new ArgumentException();

         var queryCompiler = (IQueryCompiler)_queryCompilerField.GetValue( query.Provider );
         var queryModelGenerator = (IQueryModelGenerator)_queryModelGeneratorField.GetValue( queryCompiler );
         var queryModel = queryModelGenerator.ParseQuery( query.Expression );
         var database = _databaseField.GetValue( queryCompiler );
         var queryCompilationContextFactory = ( (DatabaseDependencies)_dependenciesProperty.GetValue( database ) ).QueryCompilationContextFactory;
         var queryCompilationContext = queryCompilationContextFactory.Create( false );
         var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
         modelVisitor.CreateQueryExecutor<TEntity>( queryModel );
         return modelVisitor.Queries.Join( Environment.NewLine + Environment.NewLine );

         //var provider = query.Provider;
         //var fields = typeof( EntityQueryProvider ).GetTypeInfo().DeclaredFields;
         //var properties = typeof( EntityQueryProvider ).GetTypeInfo().DeclaredProperties;
         //var queryCompiler = QueryCompilerField.GetValue( query.Provider );
         //var nodeTypeProvider = (INodeTypeProvider)NodeTypeProviderField.GetValue( queryCompiler );
         //var parser = (IQueryParser)CreateQueryParserMethod.Invoke( queryCompiler, new object[] { nodeTypeProvider } );
         //var queryModel = parser.GetParsedQuery( query.Expression );
         //var database = DataBaseField.GetValue( queryCompiler );
         //var databaseDependencies = (DatabaseDependencies)DatabaseDependenciesProperty.GetValue( database );
         //var queryCompilationContextFactory = databaseDependencies.QueryCompilationContextFactory;
         //var queryCompilationContext = queryCompilationContextFactory.Create( false );
         //var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
         //modelVisitor.CreateQueryExecutor<TEntity>( queryModel );
         //var sql = modelVisitor.Queries.First().ToString();
         //return sql;



         //var queryCompiler = (IQueryCompiler)QueryCompilerField.GetValue( query.Provider );
         //var nodeTypeProvider = (INodeTypeProvider)NodeTypeProviderField.GetValue( queryCompiler );
         //var parser = (IQueryParser)CreateQueryParserMethod.Invoke( queryCompiler, new object[] { nodeTypeProvider } );
         //var queryModel = parser.GetParsedQuery( query.Expression );
         //var database = DataBaseField.GetValue( queryCompiler );
         //var queryCompilationContextFactory = (IQueryCompilationContextFactory)QueryCompilationContextFactoryField.GetValue( database );
         //var queryCompilationContext = queryCompilationContextFactory.Create( false );
         //var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
         //modelVisitor.CreateQueryExecutor<TEntity>( queryModel );
         //var sql = modelVisitor.Queries.First().ToString();
         //return sql;
      }
   }
}
