using TaskManagerAPI.Domain.Models;

namespace TaskManagerAPI.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
