using TaskList.Models;
using Microsoft.EntityFrameworkCore;

namespace TaskList.Data
{
    public class DatabaseContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <remarks>Step 6a</remarks>
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            // Create database and tables
            Database.EnsureCreated();
        }

        /// <summary>
        /// Represents the Tasks table (Entity Set)
        /// </summary>
        /// <value>
        /// The tasks.
        /// </value>
        public DbSet<Models.Task> Tasks { get; set; }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Associates the Task model to the Task table
            modelBuilder.Entity<Task>().ToTable("Task");
        }
    }
}
