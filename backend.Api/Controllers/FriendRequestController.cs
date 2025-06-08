using backend.Api.Services;
using backend.Api.Services.Interfaces;
using backend.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FriendRequestController : ControllerBase
{
    private readonly IFriendRequestService _friendService;
    private readonly IAuthService _authService;
    public FriendRequestController(IFriendRequestService friendService, IAuthService authService)
    {
        _friendService = friendService;
        _authService = authService;
    }


    [HttpPost("friend-request/send")]
    public async Task<IActionResult> SendFriendRequest(int receiverId)
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int senderId))
        {
            return BadRequest("Invalid sender ID.");
        }
        var result = await _friendService.SendFriendRequestAsync(senderId, receiverId);
        return result.Success ? Ok(result.Message) : BadRequest(result.Message);
    }


    [HttpPost("friend-request/reject")]
    public async Task<IActionResult> RejectFriendRequest(int requestId)
    {

        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
        {
            return BadRequest("Invalid user ID.");
        }
        var result = await _friendService.RejectFriendRequestAsync(requestId, userId);
        return result.Success ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpPost("friend-request/accept")]
    public async Task<IActionResult> AcceptFriendRequest(int requestId)
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
        {
            return BadRequest("Invalid user ID.");
        }
        var result = await _friendService.AcceptFriendRequestAsync(requestId, userId);
        return result.Success ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpGet("friend-requests/accepted")]
    public async Task<IActionResult> GetAcceptedFriendRequests()
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId)) 
        {
            return BadRequest("Invalid user ID.");
        }
        var requests = await _friendService.GetAcceptedFriendRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpGet("friend-requests/pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
        {
            return BadRequest("Invalid user ID.");
        }
        var requests = await _friendService.GetPendingRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpGet("friend-requests/sent")]
    public async Task<IActionResult> GetSentRequests()
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
        {
            return BadRequest("Invalid user ID.");
        }
        var requests = await _friendService.GetSentRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpGet("friend-requests/received")]
    public async Task<IActionResult> GetReceivedRequests()
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
        {
            return BadRequest("Invalid user ID.");
        }
        var requests = await _friendService.GetReceivedRequestsAsync(userId);
        return Ok(requests);
    }

}

