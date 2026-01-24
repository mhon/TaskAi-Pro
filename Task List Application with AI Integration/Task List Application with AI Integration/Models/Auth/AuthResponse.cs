namespace TaskAi.Api.Models.Auth;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}