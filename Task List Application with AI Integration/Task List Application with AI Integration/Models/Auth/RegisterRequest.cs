using System.Text.Json.Serialization;

namespace TaskAi.Api.Models.Auth;

public class RegisterRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;
}