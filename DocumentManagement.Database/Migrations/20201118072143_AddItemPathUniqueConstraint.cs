using Microsoft.EntityFrameworkCore.Migrations;

namespace MRS.DocumentManagement.Database.Migrations
{
    public partial class AddItemPathUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Items_Path",
                table: "Items",
                column: "Path",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_Path",
                table: "Items");
        }
    }
}
