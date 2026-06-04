using Microsoft.Extensions.Configuration;
using TaskManagerAPI.Domain.Models;
using TaskManagerAPI.Infrastructure.Services;

namespace TaskManagerAPI.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "test-secret-key-minimo-32-caracteres-aqui" },
            { "JwtSettings:Issuer", "TaskManagerAPI" },
            { "JwtSettings:Audience", "TaskManagerAPI" },
            { "JwtSettings:ExpirationHours", "2" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _tokenService = new TokenService(configuration);
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString_ForValidUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "victor@gmail.com",
            Name = "Victor Lopez"
        };

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_ReturnsDifferentTokens_ForSameUser()
    {
        // Arrange
        var user = new User { Id = 1, Email = "victor@gmail.com", Name = "Victor" };

        // Act
        var token1 = _tokenService.GenerateToken(user);
        var token2 = _tokenService.GenerateToken(user);

        // Assert — cada token tiene un Jti único
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateToken_ContainsThreeParts_ValidJwtFormat()
    {
        // Arrange
        var user = new User { Id = 1, Email = "victor@gmail.com", Name = "Victor" };

        // Act
        var token = _tokenService.GenerateToken(user);
        var parts = token.Split('.');

        // Assert — JWT siempre tiene header.payload.signature
        Assert.Equal(3, parts.Length);
    }
}