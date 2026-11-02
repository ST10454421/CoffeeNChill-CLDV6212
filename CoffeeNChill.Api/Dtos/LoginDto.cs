using System.ComponentModel.DataAnnotations;

namespace CoffeeNChill.Api.Dtos;

/// <summary>Request body for logging in.</summary>
public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}