namespace backend.Api.Errors;

public class ApiException:ApiResponse
{
    public ApiException(int statusCode, string messege = null,string details=null) : base(statusCode, messege)
    {
        Details = details;
    }

    public string Details { get; set; }
}