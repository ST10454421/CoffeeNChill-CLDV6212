using System.ComponentModel.DataAnnotations;

namespace CoffeeNChill.Api.Dtos;

/// <summary>Request body for registering a new user.</summary>
public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(8)]
    public string Password { get; set; } = "";

    [Required]
    public string FullName { get; set; } = "";

    /// <summary>Customer, Staff or Admin. Defaults to Customer.</summary>
    public string Role { get; set; } = "Customer";
}