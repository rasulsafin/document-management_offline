using Microsoft.EntityFrameworkCore;
using System;

namespace DocumentManagement.Models.Database
{
    public class DocumentManagementContext : DbContext
    {
        public DbSet<UserDb> Users { get; set; }
        public DbSet<ProjectDb> Projects { get; set; }
        public DbSet<TaskDmDb> Tasks { get; set; }
        public DbSet<TaskType> Types { get; set; }
        public DbSet<ItemDb> Items { get; set; }

        /// <summary>
        /// Bridges
        /// </summary>
        public DbSet<ProjectUsers> ProjectUsers { get; set; }
        public DbSet<ProjectItems> ProjectItems { get; set; }
        public DbSet<TaskItems> TaskItems { get; set; }

        public DocumentManagementContext(DbContextOptions<DocumentManagementContext> opt) : base(opt) { }
        public DocumentManagementContext() : base() { }

        ///// <summary>
        ///// TODO: Hide it
        ///// </summary>
        ///// <param name="options"></param>
        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //    => options.UseNpgsql("Server=127.0.0.1;Port=5432;Database=DocumentManagement;User Id=postgres;Password=123;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskDmDb>().HasKey(t => t.Id);

            modelBuilder = ModelProjectUsers(modelBuilder);

            modelBuilder = ModelProjectItems(modelBuilder);

            modelBuilder = ModelTaskItems(modelBuilder);

            ///Project to Tasks
            modelBuilder.Entity<ProjectDb>()
                .HasMany(project => project.Tasks)
                .WithOne(task => task.Project)
                .HasForeignKey(task => task.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            ///Item's enum value
            modelBuilder.Entity<ItemDb>()
                .Property(item => item.Type)
                .HasConversion(
                    e => e.ToString(),
                    e => (TypeItemDm)Enum.Parse(typeof(TypeItemDm), e));

            ///Task to Tasks
            modelBuilder.Entity<TaskDmDb>()
                .HasMany(task => task.Tasks)
                .WithOne(task => task.ParentTask)
                .HasForeignKey(task => task.ParentTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            ///Task to TypeTask
            modelBuilder.Entity<TaskDmDb>()
                .HasOne(task => task.Type)
                .WithMany();

            ///Task to User/Author
            modelBuilder.Entity<TaskDmDb>()
                .HasOne(task => task.Author)
                .WithMany(user => user.Tasks)
                .HasForeignKey(task => task.UserId);
        }

        private ModelBuilder ModelProjectUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectUsers>()
            .HasKey(x => new { x.ProjectId, x.UserId });

            modelBuilder.Entity<ProjectUsers>()
                .HasOne(x => x.Project)
                .WithMany(y => y.Users)
                .HasForeignKey(y => y.ProjectId);

            modelBuilder.Entity<ProjectUsers>()
                .HasOne(x => x.User)
                .WithMany(y => y.Projects)
                .HasForeignKey(y => y.UserId);

            return modelBuilder;
        }

        private ModelBuilder ModelProjectItems(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectItems>()
            .HasKey(x => new { x.ProjectId, x.ItemId });

            modelBuilder.Entity<ProjectItems>()
                .HasOne(x => x.Project)
                .WithMany(y => y.Items)
                .HasForeignKey(y => y.ProjectId);

            modelBuilder.Entity<ProjectItems>()
                .HasOne(x => x.Item)
                .WithMany(y => y.Projects)
                .HasForeignKey(y => y.ItemId);

            return modelBuilder;
        }

        private ModelBuilder ModelTaskItems(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItems>()
            .HasKey(x => new { x.TaskId, x.ItemId });

            modelBuilder.Entity<TaskItems>()
                .HasOne(x => x.Task)
                .WithMany(y => y.Items)
                .HasForeignKey(y => y.TaskId);

            modelBuilder.Entity<TaskItems>()
                .HasOne(x => x.Item)
                .WithMany(y => y.Tasks)
                .HasForeignKey(y => y.ItemId);

            return modelBuilder;
        }
    }
}