using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TaskList.Models
{
    public class Task
    {       
        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        /// <summary>
        /// Gets or sets the task name.
        /// </summary>
        /// <value>The task name.</value>
        [Required]
        public string taskName { get; set; }

        /// <summary>
        /// Gets or sets the completed status.
        /// </summary>
        /// <value>The completed status.</value>
        [Required]
        public bool isCompleted { get; set; }

        /// <summary>
        /// Gets or sets the due date.
        /// </summary>
        /// <value>The due date.</value>
        [Required]
        public string dueDate { get; set; }
    }
}
