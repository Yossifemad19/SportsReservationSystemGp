using backend.Api.Errors;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers.ErrorControllers;

[ApiController]
[Route("Errors/{code}")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController
{
    public ActionResult Error(int code)
    {
        return new ObjectResult(new ApiResponse(code));
    }
}