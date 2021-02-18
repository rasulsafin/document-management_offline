﻿// <auto-generated />
using System;
using MRS.DocumentManagement.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DocumentManagement.Database.Migrations
{
    [DbContext(typeof(DMContext))]
    partial class DMContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.AppProperty", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ConnectionTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionTypeID");

                    b.ToTable("AppProperties");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.AuthFieldName", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ConnectionTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionTypeID");

                    b.ToTable("AuthFieldNames");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.AuthFieldValue", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ConnectionInfoID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionInfoID");

                    b.ToTable("AuthFieldValues");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.BimElement", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ElementName")
                        .HasColumnType("TEXT");

                    b.Property<string>("GlobalID")
                        .HasColumnType("TEXT");

                    b.Property<string>("ParentName")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("BimElements");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.BimElementObjective", b =>
                {
                    b.Property<int>("BimElementID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ObjectiveID")
                        .HasColumnType("INTEGER");

                    b.HasKey("BimElementID", "ObjectiveID");

                    b.HasIndex("ObjectiveID");

                    b.ToTable("BimElementObjectives");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ConnectionInfo", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ConnectionTypeID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionTypeID");

                    b.ToTable("ConnectionInfos");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ConnectionInfoEnumerationType", b =>
                {
                    b.Property<int>("ConnectionInfoID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EnumerationTypeID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ConnectionInfoID", "EnumerationTypeID");

                    b.HasIndex("EnumerationTypeID");

                    b.ToTable("ConnectionInfoEnumerationTypes");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ConnectionInfoEnumerationValue", b =>
                {
                    b.Property<int>("ConnectionInfoID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EnumerationValueID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ConnectionInfoID", "EnumerationValueID");

                    b.HasIndex("EnumerationValueID");

                    b.ToTable("ConnectionInfoEnumerationValues");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ConnectionType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ConnectionTypes");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.DynamicField", b =>
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

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.EnumerationType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ConnectionTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExternalId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionTypeID");

                    b.ToTable("EnumerationTypes");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.EnumerationValue", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("EnumerationTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExternalId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("EnumerationTypeID");

                    b.ToTable("EnumerationValues");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.Item", b =>
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

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.Objective", b =>
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

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ObjectiveItem", b =>
                {
                    b.Property<int>("ObjectiveID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ItemID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ObjectiveID", "ItemID");

                    b.HasIndex("ItemID");

                    b.ToTable("ObjectiveItems");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ObjectiveType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ConnectionTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionTypeID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ObjectiveTypes");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.Project", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ProjectItem", b =>
                {
                    b.Property<int>("ItemID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ItemID", "ProjectID");

                    b.HasIndex("ProjectID");

                    b.ToTable("ProjectItems");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ReportCount", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Count")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.HasKey("UserID");

                    b.ToTable("ReportCounts");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.Role", b =>
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

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.User", b =>
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

                    b.HasIndex("ConnectionInfoID")
                        .IsUnique();

                    b.HasIndex("Login")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.UserProject", b =>
                {
                    b.Property<int>("ProjectID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ProjectID", "UserID");

                    b.HasIndex("UserID");

                    b.ToTable("UserProjects");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.UserRole", b =>
                {
                    b.Property<int>("UserID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoleID")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserID", "RoleID");

                    b.HasIndex("RoleID");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.AppProperty", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.ConnectionType", "ConnectionType")
                        .WithMany("AppProperties")
                        .HasForeignKey("ConnectionTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionType");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.AuthFieldName", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.ConnectionType", "ConnectionType")
                        .WithMany("AuthFieldNames")
                        .HasForeignKey("ConnectionTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionType");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.AuthFieldValue", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithMany("AuthFieldValues")
                        .HasForeignKey("ConnectionInfoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionInfo");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.BimElementObjective", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.BimElement", "BimElement")
                        .WithMany("Objectives")
                        .HasForeignKey("BimElementID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MRS.DocumentManagement.Database.Models.Objective", "Objective")
                        .WithMany("BimElements")
                        .HasForeignKey("ObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BimElement");

                    b.Navigation("Objective");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ConnectionInfo", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.ConnectionType", "ConnectionType")
                        .WithMany("ConnectionInfos")
                        .HasForeignKey("ConnectionTypeID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ConnectionType");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ConnectionInfoEnumerationType", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithMany("EnumerationTypes")
                        .HasForeignKey("ConnectionInfoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MRS.DocumentManagement.Database.Models.EnumerationType", "EnumerationType")
                        .WithMany("ConnectionInfos")
                        .HasForeignKey("EnumerationTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionInfo");

                    b.Navigation("EnumerationType");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ConnectionInfoEnumerationValue", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithMany("EnumerationValues")
                        .HasForeignKey("ConnectionInfoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MRS.DocumentManagement.Database.Models.EnumerationValue", "EnumerationValue")
                        .WithMany("ConnectionInfos")
                        .HasForeignKey("EnumerationValueID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionInfo");

                    b.Navigation("EnumerationValue");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.DynamicField", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.Objective", "Objective")
                        .WithMany("DynamicFields")
                        .HasForeignKey("ObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Objective");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.EnumerationType", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.ConnectionType", "ConnectionType")
                        .WithMany("EnumerationTypes")
                        .HasForeignKey("ConnectionTypeID");

                    b.Navigation("ConnectionType");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.EnumerationValue", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.EnumerationType", "EnumerationType")
                        .WithMany("EnumerationValues")
                        .HasForeignKey("EnumerationTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EnumerationType");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.Objective", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.User", "Author")
                        .WithMany("Objectives")
                        .HasForeignKey("AuthorID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("MRS.DocumentManagement.Database.Models.ObjectiveType", "ObjectiveType")
                        .WithMany("Objectives")
                        .HasForeignKey("ObjectiveTypeID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MRS.DocumentManagement.Database.Models.Objective", "ParentObjective")
                        .WithMany("ChildrenObjectives")
                        .HasForeignKey("ParentObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MRS.DocumentManagement.Database.Models.Project", "Project")
                        .WithMany("Objectives")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("ObjectiveType");

                    b.Navigation("ParentObjective");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ObjectiveItem", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.Item", "Item")
                        .WithMany("Objectives")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MRS.DocumentManagement.Database.Models.Objective", "Objective")
                        .WithMany("Items")
                        .HasForeignKey("ObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Item");

                    b.Navigation("Objective");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ObjectiveType", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.ConnectionType", "ConnectionType")
                        .WithMany("ObjectiveTypes")
                        .HasForeignKey("ConnectionTypeID")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("ConnectionType");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ProjectItem", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.Item", "Item")
                        .WithMany("Projects")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MRS.DocumentManagement.Database.Models.Project", "Project")
                        .WithMany("Items")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Item");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.User", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithOne("User")
                        .HasForeignKey("MRS.DocumentManagement.Database.Models.User", "ConnectionInfoID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("ConnectionInfo");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.UserProject", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.Project", "Project")
                        .WithMany("Users")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MRS.DocumentManagement.Database.Models.User", "User")
                        .WithMany("Projects")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.UserRole", b =>
                {
                    b.HasOne("MRS.DocumentManagement.Database.Models.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MRS.DocumentManagement.Database.Models.User", "User")
                        .WithMany("Roles")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.BimElement", b =>
                {
                    b.Navigation("Objectives");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ConnectionInfo", b =>
                {
                    b.Navigation("AuthFieldValues");

                    b.Navigation("EnumerationTypes");

                    b.Navigation("EnumerationValues");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ConnectionType", b =>
                {
                    b.Navigation("AppProperties");

                    b.Navigation("AuthFieldNames");

                    b.Navigation("ConnectionInfos");

                    b.Navigation("EnumerationTypes");

                    b.Navigation("ObjectiveTypes");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.EnumerationType", b =>
                {
                    b.Navigation("ConnectionInfos");

                    b.Navigation("EnumerationValues");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.EnumerationValue", b =>
                {
                    b.Navigation("ConnectionInfos");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.Item", b =>
                {
                    b.Navigation("Objectives");

                    b.Navigation("Projects");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.Objective", b =>
                {
                    b.Navigation("BimElements");

                    b.Navigation("ChildrenObjectives");

                    b.Navigation("DynamicFields");

                    b.Navigation("Items");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.ObjectiveType", b =>
                {
                    b.Navigation("Objectives");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.Project", b =>
                {
                    b.Navigation("Items");

                    b.Navigation("Objectives");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("MRS.DocumentManagement.Database.Models.User", b =>
                {
                    b.Navigation("Objectives");

                    b.Navigation("Projects");

                    b.Navigation("Roles");
                });
#pragma warning restore 612, 618
        }
    }
}
