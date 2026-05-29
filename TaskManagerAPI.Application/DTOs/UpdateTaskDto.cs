using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.DTOs
{
    public class UpdateTaskDto
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 100 caracteres")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres")]
        public string Description { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }
    }
}
