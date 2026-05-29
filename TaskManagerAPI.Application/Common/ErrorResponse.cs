namespace TaskManagerAPI.Application.Common
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Detail { get; set; } // Solo visible en desarrollo
    }
}
