using Microsoft.EntityFrameworkCore;

namespace TodoListApp.Models
{
    public class TodoDb : DbContext
    {
        public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }

        public DbSet<Project> projects { get; set; } = null!;

        public DbSet<ProjectMember> projectMembers { get; set; } = null!;

        public DbSet<User> users { get; set; } = null!;

        public DbSet<TodoList> todoLists { get; set; } = null!;

        public DbSet<Category> categories { get; set; } = null!;

        public DbSet<Status> status { get; set; } = null!;

        public DbSet<Priority> priorities { get; set; } = null!;

        public DbSet<ProjectInvitation> projectInvitations { get; set; } = null!;


        //send data
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Work" },
                new Category { CategoryId = 2, Name = "Home" },
                new Category { CategoryId = 3, Name = "Coding" },
                new Category { CategoryId = 4, Name = "Debugging" },
                new Category { CategoryId = 5, Name = "Shopping" }
            );

            modelBuilder.Entity<Status>().HasData(
                new Status { StatusId = 1, Name = "Draft" },
                new Status { StatusId = 2, Name = "Waiting Approval" },
                new Status { StatusId = 3, Name = "Approved" },
                new Status { StatusId = 4, Name = "In Progress" },
                new Status { StatusId = 5, Name = "Completed" },
                new Status { StatusId = 6, Name = "Rejected" }
            );

            modelBuilder.Entity<Priority>().HasData(
                new Priority { PriorityId = 1, Name = "Low" },
                new Priority { PriorityId = 2, Name = "Medium" },
                new Priority { PriorityId = 3, Name = "High" }
            );

            // TodoList → CreatedByUser (Restrict)
            modelBuilder.Entity<TodoList>()
                .HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TodoList → AssignedToUser (SetNull)
            modelBuilder.Entity<TodoList>()
                .HasOne(t => t.AssignedToUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectMember → User (Restrict)
            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany()
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectMember → Project (Cascade)
            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany()
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade); // Deletes all related child records automatically
        }
    }
}
