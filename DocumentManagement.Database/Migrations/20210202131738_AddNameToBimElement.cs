using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentManagement.Database.Migrations
{
    public partial class AddNameToBimElement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ElementName",
                table: "BimElements",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElementName",
                table: "BimElements");
        }
    }
}
