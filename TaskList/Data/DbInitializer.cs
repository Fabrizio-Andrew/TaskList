using TaskList.Models;
using System;
using System.Linq;

namespace TaskList.Data
{
    public class DbInitializer
    {
        /// <summary>
        /// Initializes the specified context with data
        /// </summary>
        /// <param name="context">The context.</param>
        public static void Initialize(DatabaseContext context)
        {
            // Check to see if there is any data in the customer table
            if (context.Tasks.Any())
            {
                return;
            }

            // If no data present, seed data to database
            Task[] tasks = new Task[]
            {
                new Task() {
                    taskName = "Buy groceries",
                    isCompleted = false,
                    dueDate = DateTime.Parse("2021-02-03"),
                },
                new Task() {
                    taskName = "Workout",
                    isCompleted = true,
                    dueDate = DateTime.Parse("2021-01-01"),
                },
                new Task() {
                    taskName = "Paint Fence",
                    isCompleted = false,
                    dueDate = DateTime.Parse("2021-03-15"),
                },
                new Task() {
                    taskName = "Mow Lawn",
                    isCompleted = false,
                    dueDate = DateTime.Parse("2021-06-11"),
                }
            };

            // Add the data to the in memory model
            foreach (Task c in tasks)
            {
                context.Tasks.Add(c);
            }

            // Commit the changes to the database
            context.SaveChanges();
        }
    }
}
