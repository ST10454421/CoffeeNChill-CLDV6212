using CoffeeNChill.Api.Dtos;
using CoffeeNChill.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChill.Api.Controllers;

/// <summary>Registration and login.</summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private static readonly string[] AllowedRoles = { "Customer", "Staff", "Admin" };
    private readonly UserService _users;
    private readonly TokenService _tokens;

    public AuthController(UserService users, TokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    /// <summary>Registers a user. The password is stored as a BCrypt hash.</summary>
    /// <response code="201">User created.</response>
    /// <response code="400">Invalid role or input.</response>
    /// <response code="409">Email already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var role = AllowedRoles.FirstOrDefault(r => r.Equals(dto.Role, StringComparison.OrdinalIgnoreCase));
        if (role is null) return BadRequest("Role must be Customer, Staff or Admin.");

        if (await _users.FindByEmailAsync(dto.Email) is not null)
            return Conflict("Email is already registered.");

        await _users.CreateAsync(new UserEntity
        {
            PartitionKey = role,
            RowKey = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            CreatedDate = DateTime.UtcNow
        });

        return StatusCode(StatusCodes.Status201Created, new { message = "User registered", email = dto.Email, role });
    }

    /// <summary>Validates credentials and returns a signed JWT.</summary>
    /// <response code="200">Login succeeded.</response>
    /// <response code="401">Wrong email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        var (token, expires) = _tokens.Create(user.RowKey, user.FullName, user.PartitionKey);
        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.RowKey,
            Role = user.PartitionKey,
            ExpiresAtUtc = expires
        });
    }
}