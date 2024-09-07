using Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class UpdateTaskModel
    {
        /// <summary>
        /// Update task Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Update task Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Update task Due Date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Update task Status
        /// </summary>
        public Enumerations.TaskStatus Status { get; set; }

        /// <Update>
        /// Update task Priority
        /// </summary>
        public TaskPriority Priority { get; set; }
    }
}
