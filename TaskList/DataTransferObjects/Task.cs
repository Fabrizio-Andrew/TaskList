using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskList.DataTransferObjects
{
    public class Task
    {       
        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the task name.
        /// </summary>
        /// <value>The task name.</value>
        public string TaskName { get; set; }

        /// <summary>
        /// Gets or sets the completed status.
        /// </summary>
        /// <value>The completed status.</value>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets or sets the due date.
        /// </summary>
        /// <value>The due date.</value>
        public DateTime DueDate { get; set; }
    }
}
