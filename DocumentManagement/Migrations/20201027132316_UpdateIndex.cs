using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentManagement.Migrations
{
    public partial class UpdateIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DmId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DmId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DmId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DmId",
                table: "Items");

            migrationBuilder.AddColumn<string>(
                name: "Index",
                table: "Tasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "Tasks");

            migrationBuilder.AddColumn<string>(
                name: "DmId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DmId",
                table: "Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DmId",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DmId",
                table: "Items",
                type: "text",
                nullable: true);
        }
    }
}
