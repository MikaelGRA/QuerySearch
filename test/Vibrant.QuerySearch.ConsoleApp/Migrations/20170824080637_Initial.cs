using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Vibrant.QuerySearch.ConsoleApp.Migrations
{
   public partial class Initial : Migration
   {
      protected override void Up(MigrationBuilder migrationBuilder)
      {
         migrationBuilder.CreateTable(
             name: "MyClasses",
             columns: table => new
             {
                Id = table.Column<int>(type: "int", nullable: false)
                     .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                SomeText = table.Column<string>(type: "nvarchar(max)", nullable: true)
             },
             constraints: table =>
             {
                table.PrimaryKey("PK_MyClasses", x => x.Id);
             });

         migrationBuilder.Sql("CREATE FULLTEXT CATALOG QSCatalog AS DEFAULT;", true);

         migrationBuilder.Sql(
            @"
CREATE FULLTEXT INDEX ON [MyClasses]  
(
  SomeText Language 1033
)
KEY INDEX PK_MyClasses ON QSCatalog;", true);
      }

      protected override void Down(MigrationBuilder migrationBuilder)
      {
         migrationBuilder.Sql("DROP FULLTEXT INDEX ON [MyClasses]", true);

         migrationBuilder.Sql("DROP FULLTEXT CATALOG QSCatalog AS DEFAULT;");

         migrationBuilder.DropTable(
             name: "MyClasses");
      }
   }
}
