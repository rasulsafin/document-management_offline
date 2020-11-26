using Microsoft.EntityFrameworkCore.Migrations;

namespace MRS.DocumentManagement.Database.Migrations
{
    public partial class AddRolesBimsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BimElementObjective_BimElements_BimElementID",
                table: "BimElementObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_BimElementObjective_Objectives_ObjectiveID",
                table: "BimElementObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Role_RoleID",
                table: "UserRole");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Users_UserID",
                table: "UserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRole",
                table: "UserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Role",
                table: "Role");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BimElementObjective",
                table: "BimElementObjective");

            migrationBuilder.RenameTable(
                name: "UserRole",
                newName: "UserRoles");

            migrationBuilder.RenameTable(
                name: "Role",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "BimElementObjective",
                newName: "BimElementObjectives");

            migrationBuilder.RenameIndex(
                name: "IX_UserRole_RoleID",
                table: "UserRoles",
                newName: "IX_UserRoles_RoleID");

            migrationBuilder.RenameIndex(
                name: "IX_BimElementObjective_ObjectiveID",
                table: "BimElementObjectives",
                newName: "IX_BimElementObjectives_ObjectiveID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserID", "RoleID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BimElementObjectives",
                table: "BimElementObjectives",
                columns: new[] { "BimElementID", "ObjectiveID" });

            migrationBuilder.AddForeignKey(
                name: "FK_BimElementObjectives_BimElements_BimElementID",
                table: "BimElementObjectives",
                column: "BimElementID",
                principalTable: "BimElements",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BimElementObjectives_Objectives_ObjectiveID",
                table: "BimElementObjectives",
                column: "ObjectiveID",
                principalTable: "Objectives",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleID",
                table: "UserRoles",
                column: "RoleID",
                principalTable: "Roles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserID",
                table: "UserRoles",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BimElementObjectives_BimElements_BimElementID",
                table: "BimElementObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_BimElementObjectives_Objectives_ObjectiveID",
                table: "BimElementObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleID",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserID",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BimElementObjectives",
                table: "BimElementObjectives");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "UserRole");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Role");

            migrationBuilder.RenameTable(
                name: "BimElementObjectives",
                newName: "BimElementObjective");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_RoleID",
                table: "UserRole",
                newName: "IX_UserRole_RoleID");

            migrationBuilder.RenameIndex(
                name: "IX_BimElementObjectives_ObjectiveID",
                table: "BimElementObjective",
                newName: "IX_BimElementObjective_ObjectiveID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRole",
                table: "UserRole",
                columns: new[] { "UserID", "RoleID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Role",
                table: "Role",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BimElementObjective",
                table: "BimElementObjective",
                columns: new[] { "BimElementID", "ObjectiveID" });

            migrationBuilder.AddForeignKey(
                name: "FK_BimElementObjective_BimElements_BimElementID",
                table: "BimElementObjective",
                column: "BimElementID",
                principalTable: "BimElements",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BimElementObjective_Objectives_ObjectiveID",
                table: "BimElementObjective",
                column: "ObjectiveID",
                principalTable: "Objectives",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Role_RoleID",
                table: "UserRole",
                column: "RoleID",
                principalTable: "Role",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Users_UserID",
                table: "UserRole",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
