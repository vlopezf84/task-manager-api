using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskManagerAPI.Application.DTOs;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Controllers;
using TaskManagerAPI.Domain.Models;

namespace TaskManagerAPI.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskRepository> _mockRepo;
    private readonly TasksController _controller;
    private const int UserId = 1;

    public TasksControllerTests()
    {
        _mockRepo = new Mock<ITaskRepository>();
        _controller = new TasksController(_mockRepo.Object);

        // Simula un usuario autenticado con Id = 1
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, UserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ─── GetAll ───────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithListOfTasks()
    {
        // Arrange
        var fakeTasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Task 1", UserId = UserId },
            new TaskItem { Id = 2, Title = "Task 2", UserId = UserId }
        };

        _mockRepo.Setup(r => r.GetAllAsync(UserId))
                 .ReturnsAsync(fakeTasks);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskResponseDto>>(okResult.Value);
        Assert.Equal(2, tasks.Count());
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithEmptyList_WhenNoTasksExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAllAsync(UserId))
                 .ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskResponseDto>>(okResult.Value);
        Assert.Empty(tasks);
    }

    // ─── GetById ──────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOk_WhenTaskExists()
    {
        // Arrange
        var fakeTask = new TaskItem { Id = 1, Title = "Task 1", UserId = UserId };

        _mockRepo.Setup(r => r.GetByIdAsync(1, UserId))
                 .ReturnsAsync(fakeTask);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TaskResponseDto>(okResult.Value);
        Assert.Equal("Task 1", response.Title);
    }

    [Fact]
    public async Task GetById_ThrowsNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(999, UserId))
                 .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.GetById(999)
        );
    }

    // ─── Create ───────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsCreated_WithCorrectData()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Nueva tarea",
            Description = "Descripción de prueba"
        };

        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
                 .ReturnsAsync((TaskItem task) =>
                 {
                     task.Id = 1;
                     return task;
                 });

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<TaskResponseDto>(createdResult.Value);
        Assert.Equal("Nueva tarea", response.Title);
    }

    // ─── Update ───────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsNoContent_WhenTaskExists()
    {
        // Arrange
        var fakeTask = new TaskItem { Id = 1, Title = "Task", UserId = UserId };
        var dto = new UpdateTaskDto { Title = "Actualizada", Description = "Nueva desc", IsCompleted = true };

        _mockRepo.Setup(r => r.GetByIdAsync(1, UserId))
                 .ReturnsAsync(fakeTask);

        _mockRepo.Setup(r => r.UpdateAsync(fakeTask))
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(1, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ThrowsNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(999, UserId))
                 .ReturnsAsync((TaskItem?)null);

        var dto = new UpdateTaskDto { Title = "X", Description = "Y", IsCompleted = false };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.Update(999, dto)
        );
    }

    // ─── Delete ───────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenTaskExists()
    {
        // Arrange
        var fakeTask = new TaskItem { Id = 1, Title = "Task", UserId = UserId };

        _mockRepo.Setup(r => r.GetByIdAsync(1, UserId))
                 .ReturnsAsync(fakeTask);

        _mockRepo.Setup(r => r.DeleteAsync(fakeTask))
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ThrowsNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(999, UserId))
                 .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.Delete(999)
        );
    }
}