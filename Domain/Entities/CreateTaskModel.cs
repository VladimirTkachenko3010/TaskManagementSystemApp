using Domain.Enumerations;

namespace Domain.Entities
{
    public class CreateTaskModel
    {
        /// <summary>
        /// Create task Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Create task Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Create task Due Date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Create task Status
        /// </summary>
        public Domain.Enumerations.TaskStatus Status { get; set; }

        /// <summary>
        /// Create task Priority
        /// </summary>
        public TaskPriority Priority { get; set; }
    }
}
