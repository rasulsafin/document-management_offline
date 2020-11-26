using Microsoft.EntityFrameworkCore.Migrations;

namespace MRS.DocumentManagement.Database.Migrations
{
    public partial class AddUniqueUserLoginRestriction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ConnectionInfos",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                table: "Users",
                column: "Login",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Login",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ConnectionInfos");
        }
    }
}
