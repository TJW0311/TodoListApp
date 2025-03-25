using Microsoft.EntityFrameworkCore;

namespace TodoListApp.Models
{
    public class TodoDb : DbContext
    {
        public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }

        public DbSet<User> users { get; set; } = null!;

        public DbSet<TodoList> todoLists { get; set; } = null!;

        public DbSet<Category> categories { get; set; } = null!;

        public DbSet<Status> status { get; set; } = null!;

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
                new Status { StatusId = 1, Name = "Open" },
                new Status { StatusId = 2, Name = "Completed" }
            );
        }
    }
}
