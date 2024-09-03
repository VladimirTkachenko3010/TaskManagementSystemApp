using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;


namespace TaskManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TasksController(TaskService taskService)
        {
            _taskService = taskService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskModel task)
        {
            var userId = GetCurrentUserId(); // Implement this method to get the current user's ID
            var createdTask = await _taskService.CreateTaskAsync(task, userId);
            return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(Guid id)
        {
            var userId = GetCurrentUserId(); // Implement this method to get the current user's ID
            var task = await _taskService.GetTaskByIdAsync(id, userId);

            if (task == null)
                return NotFound();

            return Ok(task);
        }


        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userId = GetCurrentUserId(); // Implement this method to get the current user's ID
            var tasks = await _taskService.GetTasksByUserIdAsync(userId);
            return Ok(tasks);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TaskModel task)
        {
            var userId = GetCurrentUserId(); // Implement this method to get the current user's ID
            task.Id = id;
            var updatedTask = await _taskService.UpdateTaskAsync(task, userId);

            if (updatedTask == null)
                return NotFound();

            return Ok(updatedTask);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var userId = GetCurrentUserId(); // Implement this method to get the current user's ID
            var result = await _taskService.DeleteTaskAsync(id, userId);

            if (!result)
                return NotFound();

            return NoContent();
        }

        // Dummy method for demonstration
        private Guid GetCurrentUserId()
        {
            // Replace with actual implementation to retrieve user ID from JWT or session
            return Guid.NewGuid();
        }
    }
}
