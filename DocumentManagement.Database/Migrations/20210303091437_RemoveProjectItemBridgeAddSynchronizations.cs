using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentManagement.Database.Migrations
{
    public partial class RemoveProjectItemBridgeAddSynchronizations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectItems");

            migrationBuilder.DropColumn(
                name: "ExternalItemId",
                table: "Items");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 952, DateTimeKind.Utc).AddTicks(9266));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Objectives",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc).AddTicks(8519));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc).AddTicks(9171));

            migrationBuilder.AddColumn<int>(
                name: "ProjectID",
                table: "Items",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "DynamicFields",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc).AddTicks(9694));

            migrationBuilder.CreateTable(
                name: "Synchronizations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Synchronizations", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_ProjectID",
                table: "Items",
                column: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Projects_ProjectID",
                table: "Items",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Projects_ProjectID",
                table: "Items");

            migrationBuilder.DropTable(
                name: "Synchronizations");

            migrationBuilder.DropIndex(
                name: "IX_Items_ProjectID",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ProjectID",
                table: "Items");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 952, DateTimeKind.Utc).AddTicks(9266),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Objectives",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc).AddTicks(8519),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc).AddTicks(9171),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

            migrationBuilder.AddColumn<string>(
                name: "ExternalItemId",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "DynamicFields",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc).AddTicks(9694),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

            migrationBuilder.CreateTable(
                name: "ProjectItems",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectItems", x => new { x.ItemID, x.ProjectID });
                    table.ForeignKey(
                        name: "FK_ProjectItems_Items_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Items",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectItems_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItems_ProjectID",
                table: "ProjectItems",
                column: "ProjectID");
        }
    }
}
