using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TaskList.DataTransferObjects
{
    public class TaskCreate
    {
        /// <summary>
        /// Gets or sets the task name.
        /// </summary>
        /// <value>The task name.</value>
        [Required]
        public string TaskName { get; set; }

        /// <summary>
        /// Gets or sets the completed status.
        /// </summary>
        /// <value>The completed status.</value>
        [Required]
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets or sets the due date.
        /// </summary>
        /// <value>The due date.</value>
        [Required]
        public DateTime DueDate { get; set; }
    }
}
