using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enumerations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using TaskManagementSystem.Controllers;

namespace Tests
{
    public class TasksControllerTests
    {
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly Mock<ILogger<TasksController>> _mockLogger;
        private readonly TasksController _controller;
        private Guid _testUserId;

        public TasksControllerTests()
        {
            _mockTaskService = new Mock<ITaskService>();
            _mockLogger = new Mock<ILogger<TasksController>>();
            _controller = new TasksController(_mockLogger.Object, _mockTaskService.Object);

            // Set up a test userId
            _testUserId = Guid.NewGuid();

            // Set up mock user context
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()) 
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task CreateTask_ReturnsCreatedAtActionResult_WhenTaskIsCreated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskModel = new CreateTaskModel
            {
                Title = "New Task",
                Description = "Task Description",
                DueDate = DateTime.Now.AddDays(1),
                Status = Domain.Enumerations.TaskStatus.Pending,
                Priority = TaskPriority.Medium
            };

            var createdTask = new TaskModel
            {
                Id = Guid.NewGuid(),
                Title = taskModel.Title,
                Description = taskModel.Description,
                DueDate = taskModel.DueDate,
                Status = taskModel.Status,
                Priority = taskModel.Priority,
                UserId = userId
            };

            _mockTaskService
                .Setup(service => service.CreateTaskAsync(It.IsAny<TaskModel>(), It.IsAny<Guid>()))
                .ReturnsAsync(createdTask);

            // Act
            var result = await _controller.CreateTask(taskModel);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(TasksController.GetTaskById), createdResult.ActionName);
            Assert.Equal(createdTask.Id, ((TaskModel)createdResult.Value).Id);
            _mockTaskService.Verify(service => service.CreateTaskAsync(It.IsAny<TaskModel>(), It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task CreateTask_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "Required");

            var taskModel = new CreateTaskModel
            {
                Description = "Task without title"
            };

            // Act
            var result = await _controller.CreateTask(taskModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateTask_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var taskModel = new CreateTaskModel
            {
                Title = "New Task",
                Description = "Task Description",
                DueDate = DateTime.Now.AddDays(1),
                Status = Domain.Enumerations.TaskStatus.Pending,
                Priority = TaskPriority.Medium
            };

            _mockTaskService
                .Setup(service => service.CreateTaskAsync(It.IsAny<TaskModel>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.CreateTask(taskModel);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Database failure", objectResult.Value);
        }

        [Fact]
        public async Task GetTaskById_ReturnsOk_WhenTaskExists()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            var task = new TaskModel
            {
                Id = taskId,
                Title = "Test Task",
                Description = "Test Description",
                UserId = _testUserId
            };

            _mockTaskService
                .Setup(service => service.GetTaskByIdAsync(taskId, _testUserId))
                .ReturnsAsync(task);

            // Act
            var result = await _controller.GetTaskById(taskId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTask = Assert.IsType<TaskModel>(okResult.Value);
            Assert.Equal(taskId, returnedTask.Id);
            _mockTaskService.Verify(service => service.GetTaskByIdAsync(taskId, _testUserId), Times.Once);
        }

        [Fact]
        public async Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockTaskService
                .Setup(service => service.GetTaskByIdAsync(taskId, _testUserId))
                .ReturnsAsync((TaskModel)null);

            // Act
            var result = await _controller.GetTaskById(taskId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockTaskService.Verify(service => service.GetTaskByIdAsync(taskId, _testUserId), Times.Once);
        }

        [Fact]
        public async Task GetTaskById_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockTaskService
                .Setup(service => service.GetTaskByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.GetTaskById(taskId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Database failure", objectResult.Value);
            _mockTaskService.Verify(service => service.GetTaskByIdAsync(taskId, _testUserId), Times.Once);
        }

        [Fact]
        public async Task GetTasks_ReturnsOk_WithTasks_WhenNoFiltersApplied()
        {
            // Arrange
            var tasks = new List<TaskModel>
            {
                new TaskModel { Id = Guid.NewGuid(), Title = "Task 1", UserId = _testUserId },
                new TaskModel { Id = Guid.NewGuid(), Title = "Task 2", UserId = _testUserId }
            };

            _mockTaskService
                .Setup(service => service.GetTasksByFiltersAsync(_testUserId, null, null, null, "DueDate", false, 1, 10))
                .ReturnsAsync(tasks);

            // Act
            var result = await _controller.GetTasks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTasks = Assert.IsType<List<TaskModel>>(okResult.Value);
            Assert.Equal(2, returnedTasks.Count);
            _mockTaskService.Verify(service => service.GetTasksByFiltersAsync(_testUserId, null, null, null, "DueDate", false, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetTasks_ReturnsOk_WithFilteredTasks()
        {
            // Arrange
            var tasks = new List<TaskModel>
            {
                new TaskModel { Id = Guid.NewGuid(), Title = "Task 1", Status = Domain.Enumerations.TaskStatus.Completed, UserId = _testUserId },
                new TaskModel { Id = Guid.NewGuid(), Title = "Task 2", Status = Domain.Enumerations.TaskStatus.Completed, UserId = _testUserId }
            };

            var status = Domain.Enumerations.TaskStatus.Completed;
            var priority = TaskPriority.High;
            var sortBy = TaskSortOptions.Priority;

            _mockTaskService
                .Setup(service => service.GetTasksByFiltersAsync(_testUserId, status, null, priority, sortBy.ToString(), true, 1, 10))
                .ReturnsAsync(tasks);

            // Act
            var result = await _controller.GetTasks(status: status, priority: priority, sortBy: sortBy, sortDescending: true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTasks = Assert.IsType<List<TaskModel>>(okResult.Value);
            Assert.Equal(2, returnedTasks.Count);
            _mockTaskService.Verify(service => service.GetTasksByFiltersAsync(_testUserId, status, null, priority, sortBy.ToString(), true, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetTasks_PaginationWorksCorrectly()
        {
            // Arrange
            var tasks = new List<TaskModel>
            {
                new TaskModel { Id = Guid.NewGuid(), Title = "Task 1", UserId = _testUserId }
            };

            _mockTaskService
                .Setup(service => service.GetTasksByFiltersAsync(_testUserId, null, null, null, "DueDate", false, 2, 5))
                .ReturnsAsync(tasks);

            // Act
            var result = await _controller.GetTasks(page: 2, pageSize: 5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTasks = Assert.IsType<List<TaskModel>>(okResult.Value);
            Assert.Single(returnedTasks);
            _mockTaskService.Verify(service => service.GetTasksByFiltersAsync(_testUserId, null, null, null, "DueDate", false, 2, 5), Times.Once);
        }

        [Fact]
        public async Task GetTasks_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            _mockTaskService
                .Setup(service => service.GetTasksByFiltersAsync(It.IsAny<Guid>(), It.IsAny<Domain.Enumerations.TaskStatus?>(), It.IsAny<DateTime?>(), It.IsAny<TaskPriority?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.GetTasks();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Database failure", objectResult.Value);
        }

        [Fact]
        public async Task UpdateTask_ReturnsOk_WhenTaskIsUpdatedSuccessfully()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var existingTask = new TaskModel
            {
                Id = taskId,
                Title = "Old Title",
                Description = "Old Description",
                UserId = _testUserId
            };

            var updatedTaskModel = new UpdateTaskModel
            {
                Title = "New Title",
                Description = "New Description",
                DueDate = DateTime.Now.AddDays(5),
                Status = Domain.Enumerations.TaskStatus.Completed,
                Priority = TaskPriority.High
            };

            var updatedTask = new TaskModel
            {
                Id = taskId,
                Title = updatedTaskModel.Title,
                Description = updatedTaskModel.Description,
                DueDate = updatedTaskModel.DueDate,
                Status = updatedTaskModel.Status,
                Priority = updatedTaskModel.Priority,
                UpdatedAt = DateTime.UtcNow,
                UserId = _testUserId
            };

            _mockTaskService
                .Setup(service => service.GetTaskByIdAsync(taskId, _testUserId))
                .ReturnsAsync(existingTask);

            _mockTaskService
                .Setup(service => service.UpdateTaskAsync(existingTask, _testUserId))
                .ReturnsAsync(updatedTask);

            // Act
            var result = await _controller.UpdateTask(taskId, updatedTaskModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTask = Assert.IsType<TaskModel>(okResult.Value);
            Assert.Equal(updatedTaskModel.Title, returnedTask.Title);
            Assert.Equal(updatedTaskModel.Description, returnedTask.Description);
            Assert.Equal(updatedTaskModel.Status, returnedTask.Status);
            Assert.Equal(updatedTaskModel.Priority, returnedTask.Priority);
            _mockTaskService.Verify(service => service.UpdateTaskAsync(existingTask, _testUserId), Times.Once);
        }

        [Fact]
        public async Task UpdateTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var updatedTaskModel = new UpdateTaskModel
            {
                Title = "New Title",
                Description = "New Description",
                DueDate = DateTime.Now.AddDays(5),
                Status = Domain.Enumerations.TaskStatus.Completed,
                Priority = TaskPriority.High
            };

            _mockTaskService
                .Setup(service => service.GetTaskByIdAsync(taskId, _testUserId))
                .ReturnsAsync((TaskModel)null);

            // Act
            var result = await _controller.UpdateTask(taskId, updatedTaskModel);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockTaskService.Verify(service => service.GetTaskByIdAsync(taskId, _testUserId), Times.Once);
        }

        [Fact]
        public async Task UpdateTask_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var updatedTaskModel = new UpdateTaskModel
            {
                Title = "New Title",
                Description = "New Description",
                DueDate = DateTime.Now.AddDays(5),
                Status = Domain.Enumerations.TaskStatus.Completed,
                Priority = TaskPriority.High
            };

            _mockTaskService
                .Setup(service => service.GetTaskByIdAsync(taskId, _testUserId))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.UpdateTask(taskId, updatedTaskModel);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Database failure", objectResult.Value);
        }

        [Fact]
        public async Task DeleteTask_TaskExists_ReturnsNoContent()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            // Set up a mock to return a successful delete result
            _mockTaskService
                .Setup(service => service.DeleteTaskAsync(taskId, _testUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteTask(taskId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_TaskDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            // Setting up a mock so the task is not found
            _mockTaskService
                .Setup(service => service.DeleteTaskAsync(taskId, _testUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteTask(taskId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_VerifyServiceCalledWithCorrectParameters()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            // Set up a mock to return a successful delete result
            _mockTaskService
                .Setup(service => service.DeleteTaskAsync(It.IsAny<Guid>(), _testUserId))
                .ReturnsAsync(true);

            // Act
            await _controller.DeleteTask(taskId);

            // Assert: we check that the method is called with the correct parameters
            _mockTaskService.Verify(service => service.DeleteTaskAsync(taskId, _testUserId), Times.Once);
        }

        [Fact]
        public void GetCurrentUserId_UserAuthenticated_ReturnsUserId()
        {
            // Act
            var result = _controller.GetCurrentUserId();

            // Assert
            Assert.Equal(_testUserId, result);
        }

        [Fact]
        public void GetCurrentUserId_UserAuthenticated_UserIdNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            // Setting up an empty ClaimsIdentity without a NameIdentifier
            var userWithoutId = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userWithoutId }
            };

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(() => _controller.GetCurrentUserId());
            Assert.Equal("User ID not found in token", exception.Message);
        }

        [Fact]
        public void GetCurrentUserId_UserNotAuthenticated_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var unauthenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = unauthenticatedUser }
            };

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(() => _controller.GetCurrentUserId());
            Assert.Equal("User ID not found in token", exception.Message);
        }

        [Fact]
        public void GetCurrentUserId_IdentityIsNotClaimsIdentity_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userWithDifferentIdentity = new ClaimsPrincipal(new GenericIdentity("TestUser"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userWithDifferentIdentity }
            };

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(() => _controller.GetCurrentUserId());
            Assert.Equal("User ID not found in token", exception.Message);
        }
    }
}
