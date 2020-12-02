using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MRS.DocumentManagement.Database.Migrations
{
    public partial class CreateDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConnectionInfos",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuthFieldNames = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionInfos", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Path = table.Column<string>(nullable: true),
                    ItemType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ObjectiveTypes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectiveTypes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EnumDms",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true),
                    ConnectionInfoID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumDms", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EnumDms_ConnectionInfos_ConnectionInfoID",
                        column: x => x.ConnectionInfoID,
                        principalTable: "ConnectionInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Login = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<byte[]>(nullable: true),
                    PasswordSalt = table.Column<byte[]>(nullable: true),
                    Role = table.Column<string>(nullable: true),
                    ConnectionInfoID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Users_ConnectionInfos_ConnectionInfoID",
                        column: x => x.ConnectionInfoID,
                        principalTable: "ConnectionInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BimElements",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemID = table.Column<int>(nullable: false),
                    GlobalID = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BimElements", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BimElements_Items_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Items",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectItems",
                columns: table => new
                {
                    ProjectID = table.Column<int>(nullable: false),
                    ItemID = table.Column<int>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "EnumDmValues",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(nullable: true),
                    EnumDmID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumDmValues", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EnumDmValues_EnumDms_EnumDmID",
                        column: x => x.EnumDmID,
                        principalTable: "EnumDms",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Objectives",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectID = table.Column<int>(nullable: false),
                    ParentObjectiveID = table.Column<int>(nullable: true),
                    AuthorID = table.Column<int>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    DueDate = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    ObjectiveTypeID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objectives", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Objectives_Users_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Objectives_ObjectiveTypes_ObjectiveTypeID",
                        column: x => x.ObjectiveTypeID,
                        principalTable: "ObjectiveTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Objectives_Objectives_ParentObjectiveID",
                        column: x => x.ParentObjectiveID,
                        principalTable: "Objectives",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Objectives_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProjects",
                columns: table => new
                {
                    UserID = table.Column<int>(nullable: false),
                    ProjectID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjects", x => new { x.ProjectID, x.UserID });
                    table.ForeignKey(
                        name: "FK_UserProjects_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProjects_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserEnumDmValues",
                columns: table => new
                {
                    EnumDmValueID = table.Column<int>(nullable: false),
                    UserID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEnumDmValues", x => new { x.EnumDmValueID, x.UserID });
                    table.ForeignKey(
                        name: "FK_UserEnumDmValues_EnumDmValues_EnumDmValueID",
                        column: x => x.EnumDmValueID,
                        principalTable: "EnumDmValues",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEnumDmValues_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BimElementObjective",
                columns: table => new
                {
                    ObjectiveID = table.Column<int>(nullable: false),
                    BimElementID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BimElementObjective", x => new { x.BimElementID, x.ObjectiveID });
                    table.ForeignKey(
                        name: "FK_BimElementObjective_BimElements_BimElementID",
                        column: x => x.BimElementID,
                        principalTable: "BimElements",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BimElementObjective_Objectives_ObjectiveID",
                        column: x => x.ObjectiveID,
                        principalTable: "Objectives",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFields",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ObjectiveID = table.Column<int>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFields", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DynamicFields_Objectives_ObjectiveID",
                        column: x => x.ObjectiveID,
                        principalTable: "Objectives",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectiveItems",
                columns: table => new
                {
                    ObjectiveID = table.Column<int>(nullable: false),
                    ItemID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectiveItems", x => new { x.ObjectiveID, x.ItemID });
                    table.ForeignKey(
                        name: "FK_ObjectiveItems_Items_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Items",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObjectiveItems_Objectives_ObjectiveID",
                        column: x => x.ObjectiveID,
                        principalTable: "Objectives",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BimElementObjective_ObjectiveID",
                table: "BimElementObjective",
                column: "ObjectiveID");

            migrationBuilder.CreateIndex(
                name: "IX_BimElements_ItemID",
                table: "BimElements",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFields_ObjectiveID",
                table: "DynamicFields",
                column: "ObjectiveID");

            migrationBuilder.CreateIndex(
                name: "IX_EnumDms_ConnectionInfoID",
                table: "EnumDms",
                column: "ConnectionInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_EnumDmValues_EnumDmID",
                table: "EnumDmValues",
                column: "EnumDmID");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectiveItems_ItemID",
                table: "ObjectiveItems",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_AuthorID",
                table: "Objectives",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_ObjectiveTypeID",
                table: "Objectives",
                column: "ObjectiveTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_ParentObjectiveID",
                table: "Objectives",
                column: "ParentObjectiveID");

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_ProjectID",
                table: "Objectives",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItems_ProjectID",
                table: "ProjectItems",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_UserEnumDmValues_UserID",
                table: "UserEnumDmValues",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_UserID",
                table: "UserProjects",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ConnectionInfoID",
                table: "Users",
                column: "ConnectionInfoID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BimElementObjective");

            migrationBuilder.DropTable(
                name: "DynamicFields");

            migrationBuilder.DropTable(
                name: "ObjectiveItems");

            migrationBuilder.DropTable(
                name: "ProjectItems");

            migrationBuilder.DropTable(
                name: "UserEnumDmValues");

            migrationBuilder.DropTable(
                name: "UserProjects");

            migrationBuilder.DropTable(
                name: "BimElements");

            migrationBuilder.DropTable(
                name: "Objectives");

            migrationBuilder.DropTable(
                name: "EnumDmValues");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ObjectiveTypes");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "EnumDms");

            migrationBuilder.DropTable(
                name: "ConnectionInfos");
        }
    }
}
