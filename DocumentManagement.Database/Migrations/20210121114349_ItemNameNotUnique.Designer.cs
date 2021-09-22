﻿// <auto-generated />
using System;
using Brio.Docs.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DocumentManagement.Database.Migrations
{
    [DbContext(typeof(DMContext))]
    [Migration("20210121114349_ItemNameNotUnique")]
    partial class ItemNameNotUnique
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Brio.Docs.Database.Models.BimElement", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("GlobalID")
                        .HasColumnType("TEXT");

                    b.Property<int>("ItemID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("ItemID");

                    b.ToTable("BimElements");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.BimElementObjective", b =>
                {
                    b.Property<int>("BimElementID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ObjectiveID")
                        .HasColumnType("INTEGER");

                    b.HasKey("BimElementID", "ObjectiveID");

                    b.HasIndex("ObjectiveID");

                    b.ToTable("BimElementObjectives");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ConnectionInfo", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AuthFieldNames")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("ConnectionInfos");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.DynamicField", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<int>("ObjectiveID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ObjectiveID");

                    b.ToTable("DynamicFields");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumDm", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ConnectionInfoID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionInfoID");

                    b.ToTable("EnumDms");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumDmValue", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("EnumDmID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("EnumDmID");

                    b.ToTable("EnumDmValues");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Item", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExternalItemId")
                        .HasColumnType("TEXT");

                    b.Property<int>("ItemType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Objective", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AuthorID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("ObjectiveTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ParentObjectiveID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("AuthorID");

                    b.HasIndex("ObjectiveTypeID");

                    b.HasIndex("ParentObjectiveID");

                    b.HasIndex("ProjectID");

                    b.ToTable("Objectives");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ObjectiveItem", b =>
                {
                    b.Property<int>("ObjectiveID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ItemID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ObjectiveID", "ItemID");

                    b.HasIndex("ItemID");

                    b.ToTable("ObjectiveItems");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ObjectiveType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ObjectiveTypes");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Project", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ProjectItem", b =>
                {
                    b.Property<int>("ItemID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ItemID", "ProjectID");

                    b.HasIndex("ProjectID");

                    b.ToTable("ProjectItems");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Role", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.User", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ConnectionInfoID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionInfoID");

                    b.HasIndex("Login")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.UserEnumDmValue", b =>
                {
                    b.Property<int>("EnumDmValueID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserID")
                        .HasColumnType("INTEGER");

                    b.HasKey("EnumDmValueID", "UserID");

                    b.HasIndex("UserID");

                    b.ToTable("UserEnumDmValues");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.UserProject", b =>
                {
                    b.Property<int>("ProjectID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ProjectID", "UserID");

                    b.HasIndex("UserID");

                    b.ToTable("UserProjects");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.UserRole", b =>
                {
                    b.Property<int>("UserID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoleID")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserID", "RoleID");

                    b.HasIndex("RoleID");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.BimElement", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.Item", "Item")
                        .WithMany("BimElements")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Item");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.BimElementObjective", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.BimElement", "BimElement")
                        .WithMany("Objectives")
                        .HasForeignKey("BimElementID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brio.Docs.Database.Models.Objective", "Objective")
                        .WithMany("BimElements")
                        .HasForeignKey("ObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BimElement");

                    b.Navigation("Objective");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.DynamicField", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.Objective", "Objective")
                        .WithMany("DynamicFields")
                        .HasForeignKey("ObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Objective");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumDm", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithMany("EnumDms")
                        .HasForeignKey("ConnectionInfoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionInfo");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumDmValue", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.EnumDm", "EnumDm")
                        .WithMany("EnumDmValues")
                        .HasForeignKey("EnumDmID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EnumDm");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Objective", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.User", "Author")
                        .WithMany("Objectives")
                        .HasForeignKey("AuthorID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Brio.Docs.Database.Models.ObjectiveType", "ObjectiveType")
                        .WithMany("Objectives")
                        .HasForeignKey("ObjectiveTypeID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Brio.Docs.Database.Models.Objective", "ParentObjective")
                        .WithMany("ChildrenObjectives")
                        .HasForeignKey("ParentObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Brio.Docs.Database.Models.Project", "Project")
                        .WithMany("Objectives")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("ObjectiveType");

                    b.Navigation("ParentObjective");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ObjectiveItem", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.Item", "Item")
                        .WithMany("Objectives")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brio.Docs.Database.Models.Objective", "Objective")
                        .WithMany("Items")
                        .HasForeignKey("ObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Item");

                    b.Navigation("Objective");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ProjectItem", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.Item", "Item")
                        .WithMany("Projects")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brio.Docs.Database.Models.Project", "Project")
                        .WithMany("Items")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Item");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.User", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithMany("Users")
                        .HasForeignKey("ConnectionInfoID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("ConnectionInfo");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.UserEnumDmValue", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.EnumDmValue", "EnumDmValue")
                        .WithMany("Users")
                        .HasForeignKey("EnumDmValueID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brio.Docs.Database.Models.User", "User")
                        .WithMany("EnumDmValues")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EnumDmValue");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.UserProject", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.Project", "Project")
                        .WithMany("Users")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brio.Docs.Database.Models.User", "User")
                        .WithMany("Projects")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.UserRole", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brio.Docs.Database.Models.User", "User")
                        .WithMany("Roles")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.BimElement", b =>
                {
                    b.Navigation("Objectives");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ConnectionInfo", b =>
                {
                    b.Navigation("EnumDms");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumDm", b =>
                {
                    b.Navigation("EnumDmValues");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumDmValue", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Item", b =>
                {
                    b.Navigation("BimElements");

                    b.Navigation("Objectives");

                    b.Navigation("Projects");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Objective", b =>
                {
                    b.Navigation("BimElements");

                    b.Navigation("ChildrenObjectives");

                    b.Navigation("DynamicFields");

                    b.Navigation("Items");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ObjectiveType", b =>
                {
                    b.Navigation("Objectives");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Project", b =>
                {
                    b.Navigation("Items");

                    b.Navigation("Objectives");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.User", b =>
                {
                    b.Navigation("EnumDmValues");

                    b.Navigation("Objectives");

                    b.Navigation("Projects");

                    b.Navigation("Roles");
                });
#pragma warning restore 612, 618
        }
    }
}
