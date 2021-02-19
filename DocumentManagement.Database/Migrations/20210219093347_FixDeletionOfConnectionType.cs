using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentManagement.Database.Migrations
{
    public partial class FixDeletionOfConnectionType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectiveTypes_ConnectionTypes_ConnectionTypeID",
                table: "ObjectiveTypes");

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectiveTypes_ConnectionTypes_ConnectionTypeID",
                table: "ObjectiveTypes",
                column: "ConnectionTypeID",
                principalTable: "ConnectionTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectiveTypes_ConnectionTypes_ConnectionTypeID",
                table: "ObjectiveTypes");

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectiveTypes_ConnectionTypes_ConnectionTypeID",
                table: "ObjectiveTypes",
                column: "ConnectionTypeID",
                principalTable: "ConnectionTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
