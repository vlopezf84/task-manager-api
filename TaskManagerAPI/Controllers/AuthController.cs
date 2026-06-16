using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.DTOs;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Domain.Models;
using TaskManagerAPI.Infrastructure.Data;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    public AuthController(AppDbContext context, ITokenService tokenService, 
        IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        // Verificar que el email no esté registrado
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (existingUser is not null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", dto.Email);
            throw new BadRequestException("Email is already registered.");
        }

        // Hashear la contraseña — nunca guardar texto plano
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = passwordHash
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        var expirationHours = int.Parse(_configuration["JwtSettings:ExpirationHours"]!);

        _logger.LogInformation("New user registered: {Email}", dto.Email);
        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", dto.Email);
            throw new BadRequestException("Invalid email or password.");
        }


        var token = _tokenService.GenerateToken(user);
        var expirationHours = int.Parse(_configuration["JwtSettings:ExpirationHours"]!);

        _logger.LogInformation("User logged in: {Email}", dto.Email);
        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
        });
    }
}