using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.DTOs;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Domain.Models;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // <- Todos los endpoints de este Controller requieren token válido
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _repository;


    public TasksController(ITaskRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves all tasks for the authenticated user.
    /// </summary>
    /// <returns>List of tasks belonging to the current user.</returns>
    /// <response code="200">Returns the list of tasks.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAll()
    {
        // throw new Exception("Error de prueba");
        var userId = GetCurrentUserId();
        var tasks = await _repository.GetAllAsync(userId);

        var response = tasks.Select(t => new TaskResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            IsCompleted = t.IsCompleted,
            CreatedAt = t.CreatedAt
        });

        return Ok(response);

    }

    /// <summary>
    /// Retrieves a specific task by ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <response code="200">Returns the task.</response>
    /// <response code="404">Task not found.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskResponseDto>> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var task = await _repository.GetByIdAsync(id, userId);
        if (task is null) throw new NotFoundException(nameof(TaskItem), id);

        return Ok(new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt
        });
    }

    /// <summary>
    /// Creates a new task for the authenticated user.
    /// </summary>
    /// <param name="dto">Task data.</param>
    /// <response code="201">Task created successfully.</response>
    /// <response code="400">Invalid input data.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskResponseDto>> Create(CreateTaskDto dto)
    {
        var userId = GetCurrentUserId();

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = userId
        };

        await _repository.CreateAsync(task);

        var response = new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, response);
    }

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    /// <param name="id">The task ID to update.</param>
    /// <param name="dto">Updated task data.</param>
    /// <response code="204">Task updated successfully.</response>
    /// <response code="404">Task not found.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, UpdateTaskDto dto)
    {
        var userId = GetCurrentUserId();
        var task = await _repository.GetByIdAsync(id, userId);
        if (task is null) throw new NotFoundException(nameof(TaskItem), id);

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.IsCompleted = dto.IsCompleted;

        await _repository.UpdateAsync(task);
        return NoContent();
    }

    /// <summary>
    /// Deletes a task.
    /// </summary>
    /// <param name="id">The task ID to delete.</param>
    /// <response code="204">Task deleted successfully.</response>
    /// <response code="404">Task not found.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var task = await _repository.GetByIdAsync(id, userId);
        if (task is null) throw new NotFoundException(nameof(TaskItem), id);

        await _repository.DeleteAsync(task);
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (userIdClaim is null)
            throw new UnauthorizedAccessException("User ID not found in token.");

        return int.Parse(userIdClaim.Value);
    }
}