﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace Vibrant.QuerySearch.Unity
{
   /// <summary>
   /// Simple extensions for making working with QuerySearch and Unity easier.
   /// </summary>
   public static class UnityContainerExtensions
   {
      /// <summary>
      /// Registers a localization service.
      /// </summary>
      /// <typeparam name="TLocalizationService"></typeparam>
      /// <param name="registry"></param>
      /// <param name="lifetimeManager"></param>
      public static void RegisterLocalizationService<TLocalizationService>(
         this IUnityContainer registry,
         LifetimeManager lifetimeManager )
         where TLocalizationService : ILocalizationService
      {
         registry.RegisterType<ILocalizationService, TLocalizationService>( lifetimeManager );
      }

      /// <summary>
      /// Registers a localization service.
      /// </summary>
      /// <typeparam name="TLocalizationService"></typeparam>
      /// <param name="registry"></param>
      public static void RegisterLocalizationService<TLocalizationService>(
         this IUnityContainer registry )
         where TLocalizationService : ILocalizationService
      {
         registry.RegisterType<ILocalizationService, TLocalizationService>();
      }

      /// <summary>
      /// Registers a query search provider.
      /// </summary>
      /// <typeparam name="TEntity"></typeparam>
      /// <typeparam name="TQuerySearchProvider"></typeparam>
      /// <param name="registry"></param>
      /// <param name="lifetimeManager"></param>
      public static void RegisterQuerySearchProvider<TEntity, TQuerySearchProvider>(
         this IUnityContainer registry,
         LifetimeManager lifetimeManager )
         where TQuerySearchProvider : IQuerySearchProvider<TEntity>
      {
         registry.RegisterType<IQuerySearchProvider<TEntity>, TQuerySearchProvider>( lifetimeManager );
      }

      /// <summary>
      /// Registers a query search provider.
      /// </summary>
      /// <typeparam name="TEntity"></typeparam>
      /// <typeparam name="TQuerySearchProvider"></typeparam>
      /// <param name="registry"></param>
      public static void RegisterQuerySearchProvider<TEntity, TQuerySearchProvider>(
         this IUnityContainer registry )
         where TQuerySearchProvider : IQuerySearchProvider<TEntity>
      {
         registry.RegisterType<IQuerySearchProvider<TEntity>, TQuerySearchProvider>();
      }
   }
}
