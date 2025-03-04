using backend.Api.Errors;
using backend.Core.Entities;
using backend.Repository.Data;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers.ErrorControllers;

[ApiController]
[Route("api/[controller]")]
public class BuggyController : ControllerBase
{
    private readonly AppDbContext _context;

    public BuggyController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("not-found")]
    public async Task<ActionResult> GetNotFoundAsync() {
            
        var product = await _context.Facilities.FindAsync(55);
        if(product == null)
        {
            return NotFound(new ApiResponse(404));
        }

        return Ok();
    }

    [HttpGet("ServerError")]
    public async Task<ActionResult<Facility>> GetServerErrorAsync()
    {
        var product = await _context.Facilities.FindAsync(55);

        var product2=product.ToString();

        return product;
    }

    [HttpGet("BadRequest")]
    public async Task<ActionResult<Facility>> GetBadRequestAsync()
    {
        return BadRequest(new ApiResponse(400));
    }

    [HttpGet("BadRequest/{id}")]
    public async Task<ActionResult<Facility>> GetNotFoundRequest(int id)
    {
        return Ok();
    }

}