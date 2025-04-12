namespace backend.Api.DTOs;

public class ResponseDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public string Message { get; set; }
    public string Role { get; internal set; }
}

public class FacilityResponseDto
{
    public string Message { get; set; }
    public object Data { get; set; } 
}

public class GetAllResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}