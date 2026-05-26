using TaskManagerAPI.Models;

namespace TaskManagerAPI.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
