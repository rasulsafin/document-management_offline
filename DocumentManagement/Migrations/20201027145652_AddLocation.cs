using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentManagement.Migrations
{
    public partial class AddLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "LocationX",
                table: "Tasks",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "LocationY",
                table: "Tasks",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "LocationZ",
                table: "Tasks",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationX",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "LocationY",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "LocationZ",
                table: "Tasks");
        }
    }
}
