using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibrant.QuerySearch.ConsoleApp.Migrations
{
    public partial class RefClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MyRefClasses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Whatver = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyRefClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MyRefClasses_MyClasses_Id",
                        column: x => x.Id,
                        principalTable: "MyClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MyRefClasses");
        }
    }
}
