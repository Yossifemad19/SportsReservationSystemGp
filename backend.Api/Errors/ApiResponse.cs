namespace backend.Api.Errors;

public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Messege { get; set; }

    public ApiResponse(int statusCode,string messege=null)
    {
        StatusCode = statusCode;
        Messege = messege ?? GetMessegeForStatusCode(statusCode);
            
    }

    private string GetMessegeForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 =>"A Bad Request,you have made",
            401 =>"Authorized , you are not",
            404 =>"Resources found , it was not",
            500 =>"Server error occured",
            _ => null
        };

    }
}