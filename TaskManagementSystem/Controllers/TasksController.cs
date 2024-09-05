using Application.Services;
using Domain.Entities;
using Domain.Enumerations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using System.Threading.Tasks;


namespace TaskManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ILogger<TasksController> logger, TaskService taskService)
        {
            _logger = logger;
            _taskService = taskService;
        }

        [Authorize]
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
            _logger.LogInformation($"Task {createdTask.Title} created for user {userId}.");

            return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        }


        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(Guid id)
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.GetTaskByIdAsync(id, userId);

            if (task == null)
                return NotFound();

            _logger.LogInformation($"Get task: {task.Title} for user {userId}.");
            return Ok(task);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetTasks(
            [FromQuery] Domain.Enumerations.TaskStatus? status = null,
            [FromQuery] DateTime? dueDate = null,
            [FromQuery] TaskPriority? priority = null,
            [FromQuery] TaskSortOptions sortBy = TaskSortOptions.DueDate,
            [FromQuery] bool sortDescending = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId(); 
            var tasks = await _taskService.GetTasksByFiltersAsync(userId, status, dueDate, priority, sortBy.ToString(), sortDescending, page, pageSize);

            foreach (var task in tasks)
            {
                _logger.LogInformation($"Get tasks by filters: {task.Title} for user {userId}.");
            }
            return Ok(tasks);
        }

        [Authorize]
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

            _logger.LogInformation($"Updated task: {updatedTask.Title} from {existingTask.Title} for user {userId}.");
            return Ok(updatedTask);
        }

        [Authorize]
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
                    _logger.LogInformation($"Claim type: {claim.Type}, value: {claim.Value}");
                }

                _logger.LogInformation($"IsAuthenticated: {User.Identity.IsAuthenticated}");
                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    _logger.LogInformation($"User ID found: {userIdClaim.Value}");
                    return Guid.Parse(userIdClaim.Value);
                }
                else
                {
                    _logger.LogInformation("User ID claim not found");
                }
            }
            else
            {
                _logger.LogInformation("Identity is not ClaimsIdentity");
            }
            throw new UnauthorizedAccessException("User ID not found in token");
        }
    }
}
