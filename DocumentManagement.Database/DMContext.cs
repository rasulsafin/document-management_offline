#define TEST // Use to perform tests
#define DEVELOPMENT //Use to work with database
#undef DEVELOPMENT

using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database.Models;

namespace MRS.DocumentManagement.Database
{
    public class DMContext : DbContext
    {
        #region Models
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

        public DbSet<ReportCount> ReportCounts { get; set; }
        #endregion

        #region Bridges
        public DbSet<ProjectItem> ProjectItems { get; set; }

        public DbSet<ObjectiveItem> ObjectiveItems { get; set; }

        public DbSet<UserProject> UserProjects { get; set; }

        public DbSet<UserEnumDmValue> UserEnumDmValues { get; set; }

        public DbSet<BimElementObjective> BimElementObjectives { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }
        #endregion

        public DMContext()
        {
        }

        public DMContext(DbContextOptions<DMContext> opt)
            : base(opt)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEVELOPMENT
            optionsBuilder.UseSqlite("Data Source = ../DocumentManagement.Api/DocumentManagement.db");
#endif

#if TEST
            optionsBuilder.UseSqlite("Data Source = DocumentManagement.db");
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			modelBuilder.Entity<User>()
				.HasOne(x => x.ConnectionInfo)
				.WithMany(x => x.Users)
				.OnDelete(DeleteBehavior.SetNull);

			// Users should have unique logins
			modelBuilder.Entity<User>()
				.HasIndex(x => x.Login)
				.IsUnique(true);

			modelBuilder.Entity<ReportCount>()
                .HasKey(x => x.UserID);

			// Roles have unique names
			modelBuilder.Entity<Role>()
				.HasIndex(x => x.Name)
				.IsUnique(true);

			modelBuilder.Entity<ObjectiveType>()
				.HasIndex(x => x.Name)
				.IsUnique(true);

			modelBuilder.Entity<UserRole>()
				.HasKey(x => new { x.UserID, x.RoleID });
			modelBuilder.Entity<UserRole>()
				.HasOne(x => x.User)
				.WithMany(x => x.Roles)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<UserRole>()
				.HasOne(x => x.Role)
				.WithMany(x => x.Users)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserProject>()
				.HasKey(x => new { x.ProjectID, x.UserID });
			modelBuilder.Entity<UserProject>()
				.HasOne(x => x.User)
				.WithMany(x => x.Projects)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<UserProject>()
				.HasOne(x => x.Project)
				.WithMany(x => x.Users)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<ProjectItem>()
				.HasKey(x => new { x.ItemID, x.ProjectID });
			modelBuilder.Entity<ProjectItem>()
				.HasOne(x => x.Item)
				.WithMany(x => x.Projects)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<ProjectItem>()
				.HasOne(x => x.Project)
				.WithMany(x => x.Items)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<ObjectiveItem>()
				.HasKey(x => new { x.ObjectiveID, x.ItemID });
			modelBuilder.Entity<ObjectiveItem>()
				.HasOne(x => x.Item)
				.WithMany(x => x.Objectives)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<ObjectiveItem>()
				.HasOne(x => x.Objective)
				.WithMany(x => x.Items)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Objective>()
				.HasOne(x => x.Project)
				.WithMany(x => x.Objectives)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<Objective>()
				.HasOne(x => x.Author)
				.WithMany(x => x.Objectives)
				.OnDelete(DeleteBehavior.SetNull);
			modelBuilder.Entity<Objective>()
				.HasOne(x => x.ParentObjective)
				.WithMany(x => x.ChildrenObjectives)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<Objective>()
				.HasOne(x => x.ObjectiveType)
				.WithMany(x => x.Objectives)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<DynamicField>()
				.HasOne(x => x.Objective)
				.WithMany(x => x.DynamicFields)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<BimElement>()
                .HasKey(x => x.ID);

			modelBuilder.Entity<BimElementObjective>()
				.HasKey(x => new { x.BimElementID, x.ObjectiveID });
			modelBuilder.Entity<BimElementObjective>()
				.HasOne(x => x.BimElement)
				.WithMany(x => x.Objectives)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<BimElementObjective>()
				.HasOne(x => x.Objective)
				.WithMany(x => x.BimElements)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<EnumDm>()
				.HasOne(x => x.ConnectionInfo)
				.WithMany(x => x.EnumDms)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<EnumDmValue>()
				.HasOne(x => x.EnumDm)
				.WithMany(x => x.EnumDmValues)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserEnumDmValue>()
				.HasKey(x => new { x.EnumDmValueID, x.UserID });
			modelBuilder.Entity<UserEnumDmValue>()
				.HasOne(x => x.User)
				.WithMany(x => x.EnumDmValues)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<UserEnumDmValue>()
				.HasOne(x => x.EnumDmValue)
				.WithMany(x => x.Users)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
