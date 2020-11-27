﻿// <auto-generated />
using System;
using MRS.DocumentManagement.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MRS.DocumentManagement.Database.Migrations
{
    [DbContext(typeof(DMContext))]
    [Migration("20201112095336_CreateDatabase")]
    partial class CreateDatabase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("DocumentManagement.Database.Models.BimElement", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("GlobalID")
                        .HasColumnType("text");

                    b.Property<int>("ItemID")
                        .HasColumnType("integer");

                    b.HasKey("ID");

                    b.HasIndex("ItemID");

                    b.ToTable("BimElements");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.BimElementObjective", b =>
                {
                    b.Property<int>("BimElementID")
                        .HasColumnType("integer");

                    b.Property<int>("ObjectiveID")
                        .HasColumnType("integer");

                    b.HasKey("BimElementID", "ObjectiveID");

                    b.HasIndex("ObjectiveID");

                    b.ToTable("BimElementObjective");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.ConnectionInfo", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("AuthFieldNames")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("ConnectionInfos");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.DynamicField", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Key")
                        .HasColumnType("text");

                    b.Property<int>("ObjectiveID")
                        .HasColumnType("integer");

                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("ObjectiveID");

                    b.ToTable("DynamicFields");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.EnumDm", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("ConnectionInfoID")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionInfoID");

                    b.ToTable("EnumDms");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.EnumDmValue", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("EnumDmID")
                        .HasColumnType("integer");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("EnumDmID");

                    b.ToTable("EnumDmValues");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.Item", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("ItemType")
                        .HasColumnType("integer");

                    b.Property<string>("Path")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.Objective", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("AuthorID")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("ObjectiveTypeID")
                        .HasColumnType("integer");

                    b.Property<int?>("ParentObjectiveID")
                        .HasColumnType("integer");

                    b.Property<int>("ProjectID")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("AuthorID");

                    b.HasIndex("ObjectiveTypeID");

                    b.HasIndex("ParentObjectiveID");

                    b.HasIndex("ProjectID");

                    b.ToTable("Objectives");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.ObjectiveItem", b =>
                {
                    b.Property<int>("ObjectiveID")
                        .HasColumnType("integer");

                    b.Property<int>("ItemID")
                        .HasColumnType("integer");

                    b.HasKey("ObjectiveID", "ItemID");

                    b.HasIndex("ItemID");

                    b.ToTable("ObjectiveItems");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.ObjectiveType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("ObjectiveTypes");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.Project", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.ProjectItem", b =>
                {
                    b.Property<int>("ItemID")
                        .HasColumnType("integer");

                    b.Property<int>("ProjectID")
                        .HasColumnType("integer");

                    b.HasKey("ItemID", "ProjectID");

                    b.HasIndex("ProjectID");

                    b.ToTable("ProjectItems");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.User", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("ConnectionInfoID")
                        .HasColumnType("integer");

                    b.Property<string>("Login")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<byte[]>("PasswordHash")
                        .HasColumnType("bytea");

                    b.Property<byte[]>("PasswordSalt")
                        .HasColumnType("bytea");

                    b.Property<string>("Role")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionInfoID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.UserEnumDmValue", b =>
                {
                    b.Property<int>("EnumDmValueID")
                        .HasColumnType("integer");

                    b.Property<int>("UserID")
                        .HasColumnType("integer");

                    b.HasKey("EnumDmValueID", "UserID");

                    b.HasIndex("UserID");

                    b.ToTable("UserEnumDmValues");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.UserProject", b =>
                {
                    b.Property<int>("ProjectID")
                        .HasColumnType("integer");

                    b.Property<int>("UserID")
                        .HasColumnType("integer");

                    b.HasKey("ProjectID", "UserID");

                    b.HasIndex("UserID");

                    b.ToTable("UserProjects");
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.BimElement", b =>
                {
                    b.HasOne("DocumentManagement.Database.Models.Item", "Item")
                        .WithMany("BimElements")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.BimElementObjective", b =>
                {
                    b.HasOne("DocumentManagement.Database.Models.BimElement", "BimElement")
                        .WithMany("Objectives")
                        .HasForeignKey("BimElementID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DocumentManagement.Database.Models.Objective", "Objective")
                        .WithMany("BimElements")
                        .HasForeignKey("ObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.DynamicField", b =>
                {
                    b.HasOne("DocumentManagement.Database.Models.Objective", "Objective")
                        .WithMany("DynamicFields")
                        .HasForeignKey("ObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.EnumDm", b =>
                {
                    b.HasOne("DocumentManagement.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithMany("EnumDms")
                        .HasForeignKey("ConnectionInfoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.EnumDmValue", b =>
                {
                    b.HasOne("DocumentManagement.Database.Models.EnumDm", "EnumDm")
                        .WithMany("EnumDmValues")
                        .HasForeignKey("EnumDmID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.Objective", b =>
                {
                    b.HasOne("DocumentManagement.Database.Models.User", "Author")
                        .WithMany("Objectives")
                        .HasForeignKey("AuthorID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("DocumentManagement.Database.Models.ObjectiveType", "ObjectiveType")
                        .WithMany("Objectives")
                        .HasForeignKey("ObjectiveTypeID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DocumentManagement.Database.Models.Objective", "ParentObjective")
                        .WithMany("ChildrenObjectives")
                        .HasForeignKey("ParentObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DocumentManagement.Database.Models.Project", "Project")
                        .WithMany("Objectives")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.ObjectiveItem", b =>
                {
                    b.HasOne("DocumentManagement.Database.Models.Item", "Item")
                        .WithMany("Objectives")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DocumentManagement.Database.Models.Objective", "Objective")
                        .WithMany("Items")
                        .HasForeignKey("ObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.ProjectItem", b =>
                {
                    b.HasOne("DocumentManagement.Database.Models.Item", "Item")
                        .WithMany("Projects")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DocumentManagement.Database.Models.Project", "Project")
                        .WithMany("Items")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.User", b =>
                {
                    b.HasOne("DocumentManagement.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithMany("Users")
                        .HasForeignKey("ConnectionInfoID")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.UserEnumDmValue", b =>
                {
                    b.HasOne("DocumentManagement.Database.Models.EnumDmValue", "EnumDmValue")
                        .WithMany("Users")
                        .HasForeignKey("EnumDmValueID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DocumentManagement.Database.Models.User", "User")
                        .WithMany("EnumDmValues")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DocumentManagement.Database.Models.UserProject", b =>
                {
                    b.HasOne("DocumentManagement.Database.Models.Project", "Project")
                        .WithMany("Users")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DocumentManagement.Database.Models.User", "User")
                        .WithMany("Projects")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
