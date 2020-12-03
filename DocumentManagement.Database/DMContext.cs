﻿using System;
using System.Security.Cryptography.X509Certificates;
using MRS.DocumentManagement.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MRS.DocumentManagement.Database
{
    public class DMContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Objective> Objectives { get; set; }
        public DbSet<ObjectiveType> ObjectiveTypes { get; set; }
        public DbSet<DynamicField> DynamicFields { get; set; }
        public DbSet<BimElement> BimElements { get; set; }
        public DbSet<ConnectionInfo> ConnectionInfos { get; set; }
        public DbSet<EnumDm> EnumDms { get; set; }
        public DbSet<EnumDmValue> EnumDmValues { get; set; }
		public DbSet<Role> Roles { get; set; }

        public DbSet<ProjectItem> ProjectItems { get; set; }
        public DbSet<ObjectiveItem> ObjectiveItems { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<UserEnumDmValue> UserEnumDmValues { get; set; }
		public DbSet<BimElementObjective> BimElementObjectives { get; set; }
		public DbSet<UserRole> UserRoles { get; set; }

        public DMContext() 
        { 
        }

        public DMContext(DbContextOptions<DMContext> opt) : base(opt)
        { 
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Server=127.0.0.1;Port=5432;Database=DocumentManagement;User Id=postgres;Password=123;");
            }
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
			b.Entity<User>()
				.HasOne(x => x.ConnectionInfo)
				.WithMany(x => x.Users)
				.OnDelete(DeleteBehavior.SetNull);
			// Users should have unique logins
			b.Entity<User>()
				.HasIndex(x => x.Login)
				.IsUnique(true);

			// Roles have unique names
			b.Entity<Role>()
				.HasIndex(x => x.Name)
				.IsUnique(true);

			// Items should have unique paths
			// TODO: Windows paths are case-insensitive,
			// and EF string comparison depends on used SQL provider
			// property collation should be defined to specify behaviour
			// EF 5.0 only, unfortunately
			// https://docs.microsoft.com/en-us/ef/core/miscellaneous/collations-and-case-sensitivity
			b.Entity<Item>()
				.HasIndex(x => x.Name)
				.IsUnique(true);

			b.Entity<ObjectiveType>()
				.HasIndex(x => x.Name)
				.IsUnique(true);

			b.Entity<UserRole>()
				.HasKey(x => new { x.UserID, x.RoleID });
			b.Entity<UserRole>()
				.HasOne(x => x.User)
				.WithMany(x => x.Roles)
				.OnDelete(DeleteBehavior.Cascade);
			b.Entity<UserRole>()
				.HasOne(x => x.Role)
				.WithMany(x => x.Users)
				.OnDelete(DeleteBehavior.Cascade);

			b.Entity<UserProject>()
				.HasKey(x => new { x.ProjectID, x.UserID });
			b.Entity<UserProject>()
				.HasOne(x => x.User)
				.WithMany(x => x.Projects)
				.OnDelete(DeleteBehavior.Cascade);
			b.Entity<UserProject>()
				.HasOne(x => x.Project)
				.WithMany(x => x.Users)
				.OnDelete(DeleteBehavior.Cascade);

			b.Entity<ProjectItem>()
				.HasKey(x => new { x.ItemID, x.ProjectID });
			b.Entity<ProjectItem>()
				.HasOne(x => x.Item)
				.WithMany(x => x.Projects)
				.OnDelete(DeleteBehavior.Cascade);
			b.Entity<ProjectItem>()
				.HasOne(x => x.Project)
				.WithMany(x => x.Items)
				.OnDelete(DeleteBehavior.Cascade);

			b.Entity<ObjectiveItem>()
				.HasKey(x => new { x.ObjectiveID, x.ItemID });
			b.Entity<ObjectiveItem>()
				.HasOne(x => x.Item)
				.WithMany(x => x.Objectives)
				.OnDelete(DeleteBehavior.Cascade);
			b.Entity<ObjectiveItem>()
				.HasOne(x => x.Objective)
				.WithMany(x => x.Items)
				.OnDelete(DeleteBehavior.Cascade);

			b.Entity<Objective>()
				.HasOne(x => x.Project)
				.WithMany(x => x.Objectives)
				.OnDelete(DeleteBehavior.Cascade);
			b.Entity<Objective>()
				.HasOne(x => x.Author)
				.WithMany(x => x.Objectives)
				.OnDelete(DeleteBehavior.SetNull);
			b.Entity<Objective>()
				.HasOne(x => x.ParentObjective)
				.WithMany(x => x.ChildrenObjectives)
				.OnDelete(DeleteBehavior.Cascade);
			b.Entity<Objective>()
				.HasOne(x => x.ObjectiveType)
				.WithMany(x => x.Objectives)
				.OnDelete(DeleteBehavior.Restrict);

			b.Entity<DynamicField>()
				.HasOne(x => x.Objective)
				.WithMany(x => x.DynamicFields)
				.OnDelete(DeleteBehavior.Cascade);

			b.Entity<BimElement>()
				.HasOne(x => x.Item)
				.WithMany(x => x.BimElements)
				.OnDelete(DeleteBehavior.Cascade);

			b.Entity<BimElementObjective>()
				.HasKey(x => new { x.BimElementID, x.ObjectiveID });
			b.Entity<BimElementObjective>()
				.HasOne(x => x.BimElement)
				.WithMany(x => x.Objectives)
				.OnDelete(DeleteBehavior.Cascade);
			b.Entity<BimElementObjective>()
				.HasOne(x => x.Objective)
				.WithMany(x => x.BimElements)
				.OnDelete(DeleteBehavior.Cascade);

			b.Entity<EnumDm>()
				.HasOne(x => x.ConnectionInfo)
				.WithMany(x => x.EnumDms)
				.OnDelete(DeleteBehavior.Cascade);

			b.Entity<EnumDmValue>()
				.HasOne(x => x.EnumDm)
				.WithMany(x => x.EnumDmValues)
				.OnDelete(DeleteBehavior.Cascade);

			b.Entity<UserEnumDmValue>()
				.HasKey(x => new { x.EnumDmValueID, x.UserID });
			b.Entity<UserEnumDmValue>()
				.HasOne(x => x.User)
				.WithMany(x => x.EnumDmValues)
				.OnDelete(DeleteBehavior.Cascade);
			b.Entity<UserEnumDmValue>()
				.HasOne(x => x.EnumDmValue)
				.WithMany(x => x.Users)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
