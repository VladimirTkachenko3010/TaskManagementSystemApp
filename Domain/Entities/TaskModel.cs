using Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TaskModel
    {
        /// <summary>
        /// Task Id
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Task Title
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Task description
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Task Due date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Task Status
        /// </summary>
        [Required]
        public Enumerations.TaskStatus Status { get; set; } = Enumerations.TaskStatus.Pending;

        /// <summary>
        /// Task Priority
        /// </summary>
        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        /// <summary>
        /// Task Created At
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Task Updated At
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relationship with User entity
        public Guid UserId { get; set; }
        public UserModel User { get; set; }
    }
}
