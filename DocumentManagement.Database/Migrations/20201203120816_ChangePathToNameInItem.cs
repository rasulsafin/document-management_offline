using Microsoft.EntityFrameworkCore.Migrations;

namespace MRS.DocumentManagement.Database.Migrations
{
    public partial class ChangePathToNameInItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Path",
                table: "Items",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Items_Path",
                table: "Items",
                newName: "IX_Items_Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Items",
                newName: "Path");

            migrationBuilder.RenameIndex(
                name: "IX_Items_Name",
                table: "Items",
                newName: "IX_Items_Path");
        }
    }
}
