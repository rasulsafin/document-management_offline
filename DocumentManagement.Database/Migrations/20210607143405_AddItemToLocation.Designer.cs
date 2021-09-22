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
    [Migration("20210607143405_AddItemToLocation")]
    partial class AddItemToLocation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("Brio.Docs.Database.Models.AppProperty", b =>
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

            modelBuilder.Entity("Brio.Docs.Database.Models.AuthFieldName", b =>
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

            modelBuilder.Entity("Brio.Docs.Database.Models.AuthFieldValue", b =>
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

            modelBuilder.Entity("Brio.Docs.Database.Models.BimElement", b =>
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

                    b.Property<int>("ConnectionTypeID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionTypeID");

                    b.ToTable("ConnectionInfos");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ConnectionInfoEnumerationType", b =>
                {
                    b.Property<int>("ConnectionInfoID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EnumerationTypeID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ConnectionInfoID", "EnumerationTypeID");

                    b.HasIndex("EnumerationTypeID");

                    b.ToTable("ConnectionInfoEnumerationTypes");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ConnectionInfoEnumerationValue", b =>
                {
                    b.Property<int>("ConnectionInfoID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EnumerationValueID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ConnectionInfoID", "EnumerationValueID");

                    b.HasIndex("EnumerationValueID");

                    b.ToTable("ConnectionInfoEnumerationValues");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ConnectionType", b =>
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

            modelBuilder.Entity("Brio.Docs.Database.Models.DynamicField", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExternalID")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSynchronized")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ObjectiveID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ParentFieldID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SynchronizationMateID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue(new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ObjectiveID");

                    b.HasIndex("ParentFieldID");

                    b.HasIndex("SynchronizationMateID")
                        .IsUnique();

                    b.ToTable("DynamicFields");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.DynamicFieldInfo", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExternalID")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ObjectiveTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ParentFieldID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ObjectiveTypeID");

                    b.HasIndex("ParentFieldID");

                    b.ToTable("DynamicFieldInfos");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumerationType", b =>
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

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumerationValue", b =>
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

            modelBuilder.Entity("Brio.Docs.Database.Models.Item", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExternalID")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSynchronized")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ItemType")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ProjectID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("RelativePath")
                        .HasColumnType("TEXT");

                    b.Property<int?>("SynchronizationMateID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue(new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

                    b.HasKey("ID");

                    b.HasIndex("ProjectID");

                    b.HasIndex("SynchronizationMateID")
                        .IsUnique();

                    b.ToTable("Items");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Location", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<float>("CameraPositionX")
                        .HasColumnType("REAL");

                    b.Property<float>("CameraPositionY")
                        .HasColumnType("REAL");

                    b.Property<float>("CameraPositionZ")
                        .HasColumnType("REAL");

                    b.Property<string>("Guid")
                        .HasColumnType("TEXT");

                    b.Property<int>("ItemID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ObjectiveID")
                        .HasColumnType("INTEGER");

                    b.Property<float>("PositionX")
                        .HasColumnType("REAL");

                    b.Property<float>("PositionY")
                        .HasColumnType("REAL");

                    b.Property<float>("PositionZ")
                        .HasColumnType("REAL");

                    b.HasKey("ID");

                    b.HasIndex("ItemID");

                    b.HasIndex("ObjectiveID")
                        .IsUnique();

                    b.ToTable("Location");
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

                    b.Property<string>("ExternalID")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSynchronized")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("LocationID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ObjectiveTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ParentObjectiveID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SynchronizationMateID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue(new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

                    b.HasKey("ID");

                    b.HasIndex("AuthorID");

                    b.HasIndex("ObjectiveTypeID");

                    b.HasIndex("ParentObjectiveID");

                    b.HasIndex("ProjectID");

                    b.HasIndex("SynchronizationMateID")
                        .IsUnique();

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

                    b.Property<int?>("ConnectionTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExternalId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ConnectionTypeID");

                    b.HasIndex("ExternalId")
                        .IsUnique();

                    b.HasIndex("Name", "ConnectionTypeID")
                        .IsUnique();

                    b.ToTable("ObjectiveTypes");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Project", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExternalID")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSynchronized")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SynchronizationMateID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue(new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

                    b.HasKey("ID");

                    b.HasIndex("SynchronizationMateID")
                        .IsUnique();

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ReportCount", b =>
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

            modelBuilder.Entity("Brio.Docs.Database.Models.Synchronization", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("Synchronizations");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.User", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ConnectionInfoID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExternalID")
                        .HasColumnType("TEXT");

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

            modelBuilder.Entity("Brio.Docs.Database.Models.AppProperty", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ConnectionType", "ConnectionType")
                        .WithMany("AppProperties")
                        .HasForeignKey("ConnectionTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionType");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.AuthFieldName", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ConnectionType", "ConnectionType")
                        .WithMany("AuthFieldNames")
                        .HasForeignKey("ConnectionTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionType");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.AuthFieldValue", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithMany("AuthFieldValues")
                        .HasForeignKey("ConnectionInfoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionInfo");
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

            modelBuilder.Entity("Brio.Docs.Database.Models.ConnectionInfo", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ConnectionType", "ConnectionType")
                        .WithMany("ConnectionInfos")
                        .HasForeignKey("ConnectionTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionType");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ConnectionInfoEnumerationType", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithMany("EnumerationTypes")
                        .HasForeignKey("ConnectionInfoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brio.Docs.Database.Models.EnumerationType", "EnumerationType")
                        .WithMany("ConnectionInfos")
                        .HasForeignKey("EnumerationTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionInfo");

                    b.Navigation("EnumerationType");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ConnectionInfoEnumerationValue", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithMany("EnumerationValues")
                        .HasForeignKey("ConnectionInfoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brio.Docs.Database.Models.EnumerationValue", "EnumerationValue")
                        .WithMany("ConnectionInfos")
                        .HasForeignKey("EnumerationValueID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectionInfo");

                    b.Navigation("EnumerationValue");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.DynamicField", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.Objective", "Objective")
                        .WithMany("DynamicFields")
                        .HasForeignKey("ObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Brio.Docs.Database.Models.DynamicField", "ParentField")
                        .WithMany("ChildrenDynamicFields")
                        .HasForeignKey("ParentFieldID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Brio.Docs.Database.Models.DynamicField", "SynchronizationMate")
                        .WithOne()
                        .HasForeignKey("Brio.Docs.Database.Models.DynamicField", "SynchronizationMateID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Objective");

                    b.Navigation("ParentField");

                    b.Navigation("SynchronizationMate");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.DynamicFieldInfo", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ObjectiveType", "ObjectiveType")
                        .WithMany("DefaultDynamicFields")
                        .HasForeignKey("ObjectiveTypeID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Brio.Docs.Database.Models.DynamicFieldInfo", "ParentField")
                        .WithMany("ChildrenDynamicFields")
                        .HasForeignKey("ParentFieldID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("ObjectiveType");

                    b.Navigation("ParentField");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumerationType", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ConnectionType", "ConnectionType")
                        .WithMany("EnumerationTypes")
                        .HasForeignKey("ConnectionTypeID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("ConnectionType");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumerationValue", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.EnumerationType", "EnumerationType")
                        .WithMany("EnumerationValues")
                        .HasForeignKey("EnumerationTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EnumerationType");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Item", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.Project", "Project")
                        .WithMany("Items")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Brio.Docs.Database.Models.Item", "SynchronizationMate")
                        .WithOne()
                        .HasForeignKey("Brio.Docs.Database.Models.Item", "SynchronizationMateID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Project");

                    b.Navigation("SynchronizationMate");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Location", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.Item", "Item")
                        .WithMany()
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Brio.Docs.Database.Models.Objective", "Objective")
                        .WithOne("Location")
                        .HasForeignKey("Brio.Docs.Database.Models.Location", "ObjectiveID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Item");

                    b.Navigation("Objective");
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

                    b.HasOne("Brio.Docs.Database.Models.Objective", "SynchronizationMate")
                        .WithOne()
                        .HasForeignKey("Brio.Docs.Database.Models.Objective", "SynchronizationMateID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Author");

                    b.Navigation("ObjectiveType");

                    b.Navigation("ParentObjective");

                    b.Navigation("Project");

                    b.Navigation("SynchronizationMate");
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

            modelBuilder.Entity("Brio.Docs.Database.Models.ObjectiveType", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ConnectionType", "ConnectionType")
                        .WithMany("ObjectiveTypes")
                        .HasForeignKey("ConnectionTypeID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("ConnectionType");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Project", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.Project", "SynchronizationMate")
                        .WithOne()
                        .HasForeignKey("Brio.Docs.Database.Models.Project", "SynchronizationMateID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("SynchronizationMate");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Synchronization", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.User", b =>
                {
                    b.HasOne("Brio.Docs.Database.Models.ConnectionInfo", "ConnectionInfo")
                        .WithOne("User")
                        .HasForeignKey("Brio.Docs.Database.Models.User", "ConnectionInfoID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("ConnectionInfo");
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
                    b.Navigation("AuthFieldValues");

                    b.Navigation("EnumerationTypes");

                    b.Navigation("EnumerationValues");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ConnectionType", b =>
                {
                    b.Navigation("AppProperties");

                    b.Navigation("AuthFieldNames");

                    b.Navigation("ConnectionInfos");

                    b.Navigation("EnumerationTypes");

                    b.Navigation("ObjectiveTypes");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.DynamicField", b =>
                {
                    b.Navigation("ChildrenDynamicFields");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.DynamicFieldInfo", b =>
                {
                    b.Navigation("ChildrenDynamicFields");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumerationType", b =>
                {
                    b.Navigation("ConnectionInfos");

                    b.Navigation("EnumerationValues");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.EnumerationValue", b =>
                {
                    b.Navigation("ConnectionInfos");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Item", b =>
                {
                    b.Navigation("Objectives");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.Objective", b =>
                {
                    b.Navigation("BimElements");

                    b.Navigation("ChildrenObjectives");

                    b.Navigation("DynamicFields");

                    b.Navigation("Items");

                    b.Navigation("Location");
                });

            modelBuilder.Entity("Brio.Docs.Database.Models.ObjectiveType", b =>
                {
                    b.Navigation("DefaultDynamicFields");

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
                    b.Navigation("Objectives");

                    b.Navigation("Projects");

                    b.Navigation("Roles");
                });
#pragma warning restore 612, 618
        }
    }
}
