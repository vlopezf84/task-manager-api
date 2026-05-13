using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    // Lista en memoria por ahora, la base de datos llega en la Semana 3
    private static List<TaskItem> _tasks = new();

    [HttpGet]
    public ActionResult<IEnumerable<TaskResponseDto>> GetAll()
    {
        var response = _tasks.Select(t => new TaskResponseDto
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
    public ActionResult<TaskItem> GetById(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null) return NotFound();
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
    public ActionResult<TaskItem> Create(CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Id = _tasks.Count + 1,
            Title = dto.Title,
            Description = dto.Description
            // IsCompleted y CreatedAt tienen valores default en el modelo
        };

        _tasks.Add(task);

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
    public IActionResult Update(int id, UpdateTaskDto dto)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null) return NotFound();

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.IsCompleted = dto.IsCompleted;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null) return NotFound();

        _tasks.Remove(task);
        return NoContent();
    }
}