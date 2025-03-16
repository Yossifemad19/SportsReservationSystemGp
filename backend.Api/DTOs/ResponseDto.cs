namespace backend.Api.DTOs;

public class ResponseDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public string Message { get; set; }
    public string Role { get; internal set; }
}
