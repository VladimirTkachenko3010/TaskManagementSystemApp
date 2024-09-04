using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


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
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskModel taskModel)
        {
            var userId = GetCurrentUserId();
            var task = new TaskModel
            {
                Title = taskModel.Title,
                Description = taskModel.Description,
                DueDate = taskModel.DueDate,
                Status = taskModel.Status,
                Priority = taskModel.Priority,
                UserId = userId
            };
            var createdTask = await _taskService.CreateTaskAsync(task, userId);
            return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(Guid id)
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.GetTaskByIdAsync(id, userId);

            if (task == null)
                return NotFound();

            return Ok(task);
        }


        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userId = GetCurrentUserId(); 
            var tasks = await _taskService.GetTasksByUserIdAsync(userId);
            return Ok(tasks);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskModel taskModel)
        {
            var userId = GetCurrentUserId();

            var existingTask = await _taskService.GetTaskByIdAsync(id, userId);
            if (existingTask == null)
            {
                return NotFound();
            }

            existingTask.Title = taskModel.Title;
            existingTask.Description = taskModel.Description;
            existingTask.DueDate = taskModel.DueDate;
            existingTask.Status = taskModel.Status;
            existingTask.Priority = taskModel.Priority;
            existingTask.UpdatedAt = DateTime.UtcNow;

            var updatedTask = await _taskService.UpdateTaskAsync(existingTask, userId);

            return Ok(updatedTask);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var userId = GetCurrentUserId(); 
            var result = await _taskService.DeleteTaskAsync(id, userId);

            if (!result)
                return NotFound();

            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            if (User.Identity is ClaimsIdentity identity)
            {
                var claims = identity.Claims.ToList();
                foreach (var claim in claims)
                {
                    Console.WriteLine($"Claim type: {claim.Type}, value: {claim.Value}");
                }

                Console.WriteLine($"IsAuthenticated: {User.Identity.IsAuthenticated}");
                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    Console.WriteLine($"User ID found: {userIdClaim.Value}");
                    return Guid.Parse(userIdClaim.Value);
                }
                else
                {
                    Console.WriteLine("User ID claim not found");
                }
            }
            else
            {
                Console.WriteLine("Identity is not ClaimsIdentity");
            }
            throw new UnauthorizedAccessException("User ID not found in token");
        }
    }
}
