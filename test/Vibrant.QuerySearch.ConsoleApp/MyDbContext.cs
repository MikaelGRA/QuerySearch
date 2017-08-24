using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Vibrant.QuerySearch.ConsoleApp
{
   public class MyDbContext : DbContext
   {
      public MyDbContext(DbContextOptions<MyDbContext> options)
         : base(options)
      {
      }

      public DbSet<MyClass> MyClasses { get; set; }
   }

   public class MyClass
   {
      public int Id { get; set; }

      public string SomeText { get; set; }
   }
   public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
   {
      public MyDbContext CreateDbContext(string[] args)
      {
         var builder = new DbContextOptionsBuilder<MyDbContext>();

         builder.UseSqlServer("Initial Catalog=MyQuerySearchDatabase;Data Source=localhost;User Id=sa;MultipleActiveResultSets=true");

         return new MyDbContext(builder.Options);
      }
   }
}
