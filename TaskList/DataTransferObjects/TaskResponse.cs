using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskList.Models;

namespace TaskList.DataTransferObjects
{
    public class TaskResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskResponse"/> class using the Task object as input.
        /// </summary>
        /// <param name="task">The Task.</param>
        public TaskResponse(Models.Task task)
        {
            id = task.id;
            taskName = task.taskName;
            isCompleted = task.isCompleted;
            dueDate = task.dueDate;
        }

        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int id { get; set; }

        /// <summary>
        /// Gets or sets the task name.
        /// </summary>
        /// <value>The task name.</value>
        public string taskName { get; set; }

        /// <summary>
        /// Gets or sets the task completed status.
        /// </summary>
        /// <value>The task completed status.</value>
        public bool isCompleted { get; set; }

        /// <summary>
        /// Gets or sets the task due date.
        /// </summary>
        /// <value>The task due date.</value>
        public DateTime dueDate { get; set; }
    }
}
