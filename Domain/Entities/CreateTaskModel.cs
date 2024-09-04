using Domain.Enumerations;

namespace Domain.Entities
{
    public class CreateTaskModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public Domain.Enumerations.TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
    }
}
