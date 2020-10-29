using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentManagement.Migrations
{
    public partial class UpdateElement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElementsJson",
                table: "Tasks");

            migrationBuilder.AddColumn<string>(
                name: "Element",
                table: "Tasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Element",
                table: "Tasks");

            migrationBuilder.AddColumn<string>(
                name: "ElementsJson",
                table: "Tasks",
                type: "text",
                nullable: true);
        }
    }
}
