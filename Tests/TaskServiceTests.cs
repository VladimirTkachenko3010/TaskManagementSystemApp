using Application.Services;
using Domain.Entities;
using Domain.Enumerations;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests
{
    public class TaskServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            var logger = new Mock<ILogger<TaskService>>().Object;
            _taskService = new TaskService(logger, _context);
        }

        [Fact]
        public async Task CreateTaskAsync_ShouldAddTaskToContext()
        {
            // Arrange
            var task = new TaskModel
            {
                Title = "Test Task",
                Description = "Test Description",
                DueDate = DateTime.UtcNow.AddDays(1),
                Status = Domain.Enumerations.TaskStatus.Completed,
                Priority = TaskPriority.High
            };
            var userId = Guid.NewGuid();

            // Act
            var result = await _taskService.CreateTaskAsync(task, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(task.Title, result.Title);
            Assert.Equal(task.Description, result.Description);
            Assert.Equal(task.DueDate, result.DueDate);
            Assert.Equal(task.Status, result.Status);
            Assert.Equal(task.Priority, result.Priority);

            // Verify that the task was actually added to the in-memory database
            var dbTask = await _context.Tasks.FindAsync(result.Id);
            Assert.NotNull(dbTask);
            Assert.Equal(task.Title, dbTask.Title);
        }

        [Fact]
        public async Task GetTasksByFiltersAsync_ShouldReturnFilteredAndSortedTasks()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tasks = new List<TaskModel>
        {
            new TaskModel { Id = Guid.NewGuid(), Title = "Task 1", Status = Domain.Enumerations.TaskStatus.Completed, DueDate = DateTime.UtcNow.AddDays(-1), Priority = TaskPriority.High, UserId = userId },
            new TaskModel { Id = Guid.NewGuid(), Title = "Task 2", Status = Domain.Enumerations.TaskStatus.Pending, DueDate = DateTime.UtcNow.AddDays(1), Priority = TaskPriority.Low, UserId = userId },
            new TaskModel { Id = Guid.NewGuid(), Title = "Task 3", Status = Domain.Enumerations.TaskStatus.Completed, DueDate = DateTime.UtcNow, Priority = TaskPriority.Medium, UserId = userId }
        };

            await _context.Tasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskService.GetTasksByFiltersAsync(
                userId,
                Domain.Enumerations.TaskStatus.Completed,
                null,
                null,
                "duedate",
                false,
                1,
                10
            );

            // Assert
            Assert.Equal(2, result.Count()); // Two tasks should match the filter
            Assert.Equal("Task 1", result.First().Title); // Verify sorting by DueDate
        }

        [Fact]
        public async Task GetTasksByFiltersAsync_ShouldReturnPaginatedResults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tasks = new List<TaskModel>
        {
            new TaskModel { Id = Guid.NewGuid(), Title = "Task 1", UserId = userId },
            new TaskModel { Id = Guid.NewGuid(), Title = "Task 2", UserId = userId },
            new TaskModel { Id = Guid.NewGuid(), Title = "Task 3", UserId = userId }
        };

            await _context.Tasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskService.GetTasksByFiltersAsync(
                userId,
                null,
                null,
                null,
                "title",
                false,
                2, // Page number
                1  // Page size
            );

            // Assert
            Assert.Single(result); // Only one task should be in the second page
            Assert.Equal("Task 2", result.First().Title);
        }

        [Fact]
        public async Task GetTasksByFiltersAsync_ShouldReturnEmptyListForNoMatchingTasks()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _taskService.GetTasksByFiltersAsync(
                userId,
                Domain.Enumerations.TaskStatus.Completed,
                DateTime.UtcNow.AddDays(10),
                TaskPriority.High,
                "duedate",
                true,
                1,
                10
            );

            // Assert
            Assert.Empty(result); // No tasks should match the filter
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnTaskIfExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = new TaskModel
            {
                Id = Guid.NewGuid(),
                Title = "Test Task",
                UserId = userId
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskService.GetTaskByIdAsync(task.Id, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(task.Id, result.Id);
            Assert.Equal(task.Title, result.Title);
            Assert.Equal(task.UserId, result.UserId);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnNullIfTaskDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var nonExistentTaskId = Guid.NewGuid();

            // Act
            var result = await _taskService.GetTaskByIdAsync(nonExistentTaskId, userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnNullIfTaskDoesNotBelongToUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var task = new TaskModel
            {
                Id = Guid.NewGuid(),
                Title = "Test Task",
                UserId = userId
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskService.GetTaskByIdAsync(task.Id, otherUserId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTasksByUserIdAsync_ShouldReturnTasksForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tasks = new List<TaskModel>
        {
            new TaskModel { Id = Guid.NewGuid(), Title = "Task 1", UserId = userId },
            new TaskModel { Id = Guid.NewGuid(), Title = "Task 2", UserId = userId }
        };

            await _context.Tasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskService.GetTasksByUserIdAsync(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, t => t.Title == "Task 1");
            Assert.Contains(result, t => t.Title == "Task 2");
        }

        [Fact]
        public async Task GetTasksByUserIdAsync_ShouldReturnEmptyListIfNoTasks()
        {
            // Arrange
            var userId = Guid.NewGuid(); // No tasks for this user

            // Act
            var result = await _taskService.GetTasksByUserIdAsync(userId);

            // Assert
            Assert.Empty(result); // No tasks should be found for this user
        }

        [Fact]
        public async Task GetTasksByUserIdAsync_ShouldReturnEmptyListForIncorrectUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var tasks = new List<TaskModel>
        {
            new TaskModel { Id = Guid.NewGuid(), Title = "Task 1", UserId = otherUserId },
            new TaskModel { Id = Guid.NewGuid(), Title = "Task 2", UserId = otherUserId }
        };

            await _context.Tasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskService.GetTasksByUserIdAsync(userId);

            // Assert
            Assert.Empty(result); // No tasks should be found for the incorrect user
        }


        [Fact]
        public async Task UpdateTaskAsync_ShouldUpdateTaskIfExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = new TaskModel
            {
                Id = Guid.NewGuid(),
                Title = "Old Title",
                UserId = userId
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            var updatedTask = new TaskModel
            {
                Id = task.Id,
                Title = "New Title",
                Description = "Updated Description",
                DueDate = DateTime.UtcNow.AddDays(1),
                Status = Domain.Enumerations.TaskStatus.Completed,
                Priority = TaskPriority.High,
                UserId = userId
            };

            // Act
            var result = await _taskService.UpdateTaskAsync(updatedTask, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedTask.Title, result.Title);
            Assert.Equal(updatedTask.Description, result.Description);
            Assert.Equal(updatedTask.DueDate, result.DueDate);
            Assert.Equal(updatedTask.Status, result.Status);
            Assert.Equal(updatedTask.Priority, result.Priority);
            Assert.Equal(updatedTask.UserId, result.UserId);
            Assert.True(result.UpdatedAt > task.CreatedAt); // Check that UpdatedAt was updated
        }

        [Fact]
        public async Task UpdateTaskAsync_ShouldReturnNullIfTaskDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updatedTask = new TaskModel
            {
                Id = Guid.NewGuid(), // Non-existent task ID
                Title = "New Title",
                UserId = userId
            };

            // Act
            var result = await _taskService.UpdateTaskAsync(updatedTask, userId);

            // Assert
            Assert.Null(result); // Should return null if the task does not exist
        }

        [Fact]
        public async Task UpdateTaskAsync_ShouldReturnNullIfTaskDoesNotBelongToUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var task = new TaskModel
            {
                Id = Guid.NewGuid(),
                Title = "Old Title",
                UserId = otherUserId
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            var updatedTask = new TaskModel
            {
                Id = task.Id,
                Title = "New Title",
                UserId = userId // Different user ID
            };

            // Act
            var result = await _taskService.UpdateTaskAsync(updatedTask, userId);

            // Assert
            Assert.Null(result); // Should return null if the task does not belong to the user
        }


        [Fact]
        public async Task DeleteTaskAsync_ShouldReturnTrueIfTaskExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = new TaskModel
            {
                Id = Guid.NewGuid(),
                Title = "Task to delete",
                UserId = userId
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskService.DeleteTaskAsync(task.Id, userId);

            // Assert
            Assert.True(result); // Should return true when the task is successfully deleted
            Assert.Null(await _context.Tasks.FindAsync(task.Id)); // Task should be removed from the database
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldReturnFalseIfTaskDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var nonExistentTaskId = Guid.NewGuid(); // Task ID that does not exist

            // Act
            var result = await _taskService.DeleteTaskAsync(nonExistentTaskId, userId);

            // Assert
            Assert.False(result); // Should return false if the task does not exist
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldReturnFalseIfTaskDoesNotBelongToUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var task = new TaskModel
            {
                Id = Guid.NewGuid(),
                Title = "Task for another user",
                UserId = otherUserId
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskService.DeleteTaskAsync(task.Id, userId);

            // Assert
            Assert.False(result); // Should return false if the task does not belong to the user
        }
    }
}
