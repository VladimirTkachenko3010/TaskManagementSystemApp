using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }


        // Create a new task
        public async Task<TaskModel> CreateTaskAsync(TaskModel task, Guid userId)
        {
            task.UserId = userId;
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskModel> GetTaskByIdAsync(Guid taskId, Guid userId)
        {
            return await _context.Tasks
                .Where(t => t.Id == taskId && t.UserId == userId)
                .FirstOrDefaultAsync();
        }


        // Read tasks for a specific user
        public async Task<IEnumerable<TaskModel>> GetTasksByUserIdAsync(Guid userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }


        // Update a task
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


        // Delete a task
        public async Task<bool> DeleteTaskAsync(Guid taskId, Guid userId)
        {
            var task = await _context.Tasks
                .Where(t => t.Id == taskId && t.UserId == userId)
                .FirstOrDefaultAsync();

            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
