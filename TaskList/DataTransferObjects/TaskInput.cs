using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using TaskList.Data;

namespace TaskList.DataTransferObjects
{
    public class TaskInput
    {
        /// <summary>
        /// Gets or sets the task name.
        /// </summary>
        /// <value>The task name.</value>
        [Required(ErrorMessage = "3")]
        [StringLength(100, ErrorMessage = "2")]
        public string taskName { get; set; }

        /// <summary>
        /// Gets or sets the completed status.
        /// </summary>
        /// <value>The completed status.</value>
        [Required(ErrorMessage = "3")]
        public bool isCompleted { get; set; }

        /// <summary>
        /// Gets or sets the due date.
        /// </summary>
        /// <value>The due date.</value>
        [Required(ErrorMessage = "3")]
        public DateTime dueDate { get; set; }

        /// <summary>
        /// Returns a boolean to represent if a taskName submitted via POST or PATCH is unique.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="context"></param>
        /// <param name="id"></param>
        public static bool CheckUniqueTaskName(TaskInput payload, DatabaseContext context, int id = 0)
        {
            // Validate that the incoming task name does not conflict with an existing task in the db
            Models.Task existingTask = (from c in context.Tasks where c.taskName == payload.taskName select c).SingleOrDefault();

            if (existingTask != null)
            {
                if (payload.taskName == existingTask.taskName && existingTask.id != id)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
