namespace CoffeeNChill.Api.Dtos;

/// <summary>Returned after a successful login.</summary>
public class AuthResponseDto
{
    public string Token { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime ExpiresAtUtc { get; set; }
}