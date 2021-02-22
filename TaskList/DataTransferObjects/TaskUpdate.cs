using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace TaskList.DataTransferObjects
{
    public class TaskUpdate
    {
        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Required(ErrorMessage = "3")]
        public int id { get; set; }

        /// <summary>
        /// Gets or sets the task name.
        /// </summary>
        /// <value>The task name.</value>
        [Required(ErrorMessage = "3")]
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
    }
}
