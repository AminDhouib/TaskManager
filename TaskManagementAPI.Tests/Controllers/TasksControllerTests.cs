using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementAPI.Controllers;
using TaskManagementAPI.Data;
using ModelTask = TaskManagementAPI.Models.Task;
using Xunit;

namespace TaskManagementAPI.Tests.Controllers
{
    public class TasksControllerTests
    {
        private readonly TaskContext _context;
        private readonly TasksController _controller;

        public TasksControllerTests()
        {
            var options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(databaseName: "TaskDatabase")
                .Options;

            _context = new TaskContext(options);
            _controller = new TasksController(_context);

            // Ensure the database is empty before each test
            _context.Tasks.RemoveRange(_context.Tasks);
            _context.SaveChanges();

            // Seed the in-memory database with a single task
            _context.Tasks.Add(new ModelTask
            {
                Id = System.Guid.NewGuid().ToString(),
                Title = "Test Task",
                Description = "Test Description",
                IsCompleted = false
            });
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetTasks_ReturnsTasks()
        {
            // Act
            var result = await _controller.GetTasks();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ModelTask>>>(result);
            var returnValue = Assert.IsType<List<ModelTask>>(actionResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetTask_ReturnsTask()
        {
            // Arrange
            var task = _context.Tasks.First();

            // Act
            var result = await _controller.GetTask(task.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ModelTask>>(result);
            var returnValue = Assert.IsType<ModelTask>(actionResult.Value);
            Assert.Equal(task.Id, returnValue.Id);
        }

        [Fact]
        public async Task PostTask_CreatesTask()
        {
            // Arrange
            var newTask = new ModelTask
            {
                Title = "New Task",
                Description = "New Description",
                IsCompleted = false
            };

            // Act
            var result = await _controller.PostTask(newTask);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ModelTask>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<ModelTask>(createdAtActionResult.Value);
            Assert.Equal(newTask.Title, returnValue.Title);
        }

        [Fact]
        public async Task PutTask_UpdatesTask()
        {
            // Arrange
            var task = _context.Tasks.First();
            task.IsCompleted = true;

            // Act
            var result = await _controller.PutTask(task.Id, task);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedTask = _context.Tasks.Find(task.Id);
            Assert.True(updatedTask.IsCompleted);
        }
    }
}
