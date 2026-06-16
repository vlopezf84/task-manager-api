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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAll()
    { NO COMPILA
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

    [HttpGet("{id}")]
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

    [HttpPost]
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

    [HttpPut("{id}")]
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

    [HttpDelete("{id}")]
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