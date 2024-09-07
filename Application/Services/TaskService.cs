using Application.Interfaces;
using Domain.Entities;
using Domain.Enumerations;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaskService> _logger;


        public TaskService(ILogger<TaskService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Create a new task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<TaskModel> CreateTaskAsync(TaskModel task, Guid userId)
        {
            task.UserId = userId;
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

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
        public async Task<IEnumerable<TaskModel>> GetTasksByFiltersAsync(
            Guid userId,
            Domain.Enumerations.TaskStatus? status,
            DateTime? dueDate,
            TaskPriority? priority,
            string sortBy,
            bool sortDescending,
            int page,
            int pageSize)
        {
            var query = _context.Tasks.Where(t => t.UserId == userId);

            // Filtration
            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (dueDate.HasValue)
            {
                query = query.Where(t => t.DueDate.Value.Date == dueDate.Value.Date);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            // Sorting
            query = sortBy.ToLower() switch
            {
                "duedate" => sortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
                "priority" => sortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                _ => sortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate)
            };

            // Pagination
            var skip = (page - 1) * pageSize;
            query = query.Skip(skip).Take(pageSize);

            return await query.ToListAsync();
        }


        /// <summary>
        /// get tasks by id
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<TaskModel> GetTaskByIdAsync(Guid taskId, Guid userId)
        {
            return await _context.Tasks
                .Where(t => t.Id == taskId && t.UserId == userId)
                .FirstOrDefaultAsync();
        }



        /// <summary>
        /// Read tasks for a specific user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TaskModel>> GetTasksByUserIdAsync(Guid userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }



        /// <summary>
        /// Update a task
        /// </summary>
        /// <param name="updatedTask"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<TaskModel> UpdateTaskAsync(TaskModel updatedTask, Guid userId)
        {
            var task = await _context.Tasks
                .Where(t => t.Id == updatedTask.Id && t.UserId == userId)
                .FirstOrDefaultAsync();

            if (task == null) return null;

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.DueDate = updatedTask.DueDate;
            task.Status = updatedTask.Status;
            task.Priority = updatedTask.Priority;
            task.UpdatedAt = DateTime.UtcNow;

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            return task;
        }



        /// <summary>
        /// Delete a task
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteTaskAsync(Guid taskId, Guid userId)
        {
            var task = await _context.Tasks
                .Where(t => t.Id == taskId && t.UserId == userId)
                .FirstOrDefaultAsync();

            if (task == null) return false;

            _logger.LogInformation($"Task {task.Title} deleted for user {userId}.");
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
