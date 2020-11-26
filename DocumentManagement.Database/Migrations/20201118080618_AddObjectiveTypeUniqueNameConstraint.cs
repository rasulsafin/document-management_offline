using Microsoft.EntityFrameworkCore.Migrations;

namespace MRS.DocumentManagement.Database.Migrations
{
    public partial class AddObjectiveTypeUniqueNameConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ObjectiveTypes_Name",
                table: "ObjectiveTypes",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ObjectiveTypes_Name",
                table: "ObjectiveTypes");
        }
    }
}
