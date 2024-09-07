using Domain.Entities;
using Domain.Enumerations;

namespace Application.Interfaces
{
    public interface ITaskService
    {
        /// <summary>
        /// create task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<TaskModel> CreateTaskAsync(TaskModel task, Guid userId);
        
        /// <summary>
        /// get tasks with filters
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        /// <param name="dueDate"></param>
        /// <param name="priority"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortDescending"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<IEnumerable<TaskModel>> GetTasksByFiltersAsync(
            Guid userId,
            Domain.Enumerations.TaskStatus? status,
            DateTime? dueDate,
            TaskPriority? priority,
            string sortBy,
            bool sortDescending,
            int page,
            int pageSize);
        
        /// <summary>
        /// get task by id
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<TaskModel> GetTaskByIdAsync(Guid taskId, Guid userId);
        
        /// <summary>
        /// get all tasks 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<TaskModel>> GetTasksByUserIdAsync(Guid userId);
        
        /// <summary>
        /// Update task
        /// </summary>
        /// <param name="updatedTask"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<TaskModel> UpdateTaskAsync(TaskModel updatedTask, Guid userId);
        
        /// <summary>
        /// delete task
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> DeleteTaskAsync(Guid taskId, Guid userId);
    }
}
