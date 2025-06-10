using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using backend.Api.DTOs;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using backend.Api.Errors;

namespace backend.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly IMatchService _matchService;
        private readonly IMapper _mapper;
        private readonly ILogger<MatchController> _logger;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MatchController(IMatchService matchService, IMapper mapper, ILogger<MatchController> logger, IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
        {
            _matchService = matchService;
            _mapper = mapper;
            _logger = logger;
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMatch([FromBody] CreateMatchRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var booking = await _bookingRepository.GetBookingWithDetailsAsync(request.BookingId);
                if (booking == null)
                {
                    return NotFound(new ApiResponse(404, "Booking not found"));
                }

                if (booking.UserId != userId)
                {
                    return BadRequest(new ApiResponse(400, "You can only create matches for your own bookings"));
                }

                if (booking.Court == null)
                {
                    return BadRequest(new ApiResponse(400, "The booking's court information is missing"));
                }

                var sportId = booking.Court.SportId;
                var match = await _matchService.CreateMatchAsync(
                    userId,
                    request.BookingId,
                    sportId,
                    request.TeamSize,
                    request.Title,
                    request.Description,
                    request.MinSkillLevel,
                    request.MaxSkillLevel
                );

                var matchDto = _mapper.Map<MatchDto>(match);
                return Ok(matchDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating match");
                return StatusCode(500, new ApiResponse(500, "An error occurred while creating the match"));
            }
        }

        [HttpPost("{matchId}/leave")]
        public async Task<IActionResult> LeaveMatch(int matchId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                await _matchService.LeaveMatchAsync(matchId, userId);
                return Ok("Successfully left the match");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Leave match validation failed for match {MatchId}", matchId);
                return BadRequest(new ApiResponse(400, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while leaving match {MatchId}", matchId);
                return StatusCode(500, new ApiResponse(500, "An error occurred while leaving the match"));
            }
        }

        [HttpPost("{matchId}/kick/{targetUserId}")]
        public async Task<IActionResult> KickPlayer(int matchId, int targetUserId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);

                await _matchService.KickPlayerAsync(matchId, userId, targetUserId);
                return Ok("Player kicked successfully");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to kick player {TargetUserId} from match {MatchId}", targetUserId, matchId);
                return Forbid("Only the match creator can kick players");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while kicking player {TargetUserId} from match {MatchId}", targetUserId, matchId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while kicking player {TargetUserId} from match {MatchId}", targetUserId, matchId);
                return StatusCode(500,"An error occurred while kicking the player");
            }
        }



        [HttpGet("{matchId}")]
        public async Task<IActionResult> GetMatch(int matchId)
        {
            try
            {
                var match = await _matchService.GetMatchByIdAsync(matchId);
                if (match == null)
                {
                    return NotFound("Match not found");
                }
                
                var matchDto = _mapper.Map<MatchDto>(match);
                return Ok(matchDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while getting the match");
            }
        }

        [HttpGet("my-matches")]
        public async Task<IActionResult> GetMyMatches()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var matches = await _matchService.GetMatchesByUserIdAsync(userId);
                
                var matchDtos = _mapper.Map<IEnumerable<MatchDto>>(matches);
                return Ok(matchDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user matches");
                return StatusCode(500, "An error occurred while getting your matches");
            }
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableMatches([FromQuery] int? sportId = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var matches = await _matchService.GetAvailableMatchesAsync(userId, sportId);
                return Ok(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available matches");
                return StatusCode(500, "An error occurred while getting available matches");
            }
        }

        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedMatches()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var matches = await _matchService.GetCompletedMatchesAsync(userId);
                
                var matchDtos = _mapper.Map<IEnumerable<MatchDto>>(matches);
                return Ok(matchDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed matches");
                return StatusCode(500, "An error occurred while getting completed matches");
            }
        }

        [HttpPost("{matchId}/cancel")]
        public async Task<IActionResult> CancelMatch(int matchId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var result = await _matchService.CancelMatchAsync(matchId, userId);
                
                return Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while cancelling the match");
            }
        }

        [HttpPost("{matchId}/complete")]
        public async Task<IActionResult> CompleteMatch(int matchId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                
                // Check if user is match creator
                var match = await _matchService.GetMatchByIdAsync(matchId);
                if (match == null)
                {
                    return NotFound("Match not found");
                }
                
                if (match.CreatorUserId != userId)
                {
                    // Use StatusCode(403) with a message instead of Forbid()
                    return StatusCode(403, "Only the match creator can complete the match");
                }
                
                var result = await _matchService.CompleteMatchAsync(matchId);
                
                return Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while completing the match");
            }
        }

        [HttpPost("{matchId}/invite")]
        public async Task<IActionResult> InvitePlayer(int matchId, [FromBody] InvitePlayerRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var result = await _matchService.InvitePlayerToMatchAsync(matchId, request.InvitedUserId, userId);
                
                return Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting player to match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while inviting the player");
            }
        }

        [HttpPost("{matchId}/respond-invitation")]
        public async Task<IActionResult> RespondToInvitation(int matchId, [FromBody] RespondInvitationRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var result = await _matchService.RespondToInvitationAsync(matchId, userId, request.Accept);
                
                return Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to invitation for match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while responding to the invitation");
            }
        }

        [HttpPost("{matchId}/join")]
        public async Task<IActionResult> RequestToJoin(int matchId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var result = await _matchService.JoinMatchAsync(matchId, userId);

                return Ok(new
                {
                    Success = true,
                    Message = "You have successfully joined the match."
                });

            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting to join match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while requesting to join the match");
            }
        }

        //[HttpPost("{matchId}/respond-join-request")]
        //public async Task<IActionResult> RespondToJoinRequest(int matchId, [FromBody] RespondJoinRequest request)
        //{
        //    try
        //    {
        //        var userId = int.Parse(User.FindFirst("sub")?.Value);
        //        var result = await _matchService.RespondToJoinRequestAsync(matchId, request.RequesterId, userId, request.Approve);
                
        //        return Ok(new { Success = result });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error responding to join request for match {MatchId}", matchId);
        //        return StatusCode(500, "An error occurred while responding to the join request");
        //    }
        //}

        [HttpPost("{matchId}/check-in")]
        public async Task<IActionResult> CheckIn(int matchId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var result = await _matchService.CheckInPlayerAsync(matchId, userId);
                
                return Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in for match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while checking in");
            }
        }

        [HttpGet("{matchId}/players")]
        public async Task<IActionResult> GetMatchPlayers(int matchId)
        {
            try
            {
                var players = await _matchService.GetMatchPlayersAsync(matchId);
                
                var playerDtos = _mapper.Map<IEnumerable<MatchPlayerDto>>(players);
                return Ok(playerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting players for match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while getting match players");
            }
        }

        [HttpPost("{matchId}/assign-team")]
        public async Task<IActionResult> AssignTeam(int matchId, [FromBody] AssignTeamRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                
                // First check if user is match creator
                var match = await _matchService.GetMatchByIdAsync(matchId);
                if (match == null)
                {
                    return NotFound("Match not found");
                }
                
                if (match.CreatorUserId != userId)
                {
                    // Use StatusCode(403) with a message instead of Forbid()
                    return StatusCode(403, "Only the match creator can assign teams");
                }
                
                var result = await _matchService.AssignTeamAsync(matchId, request.PlayerId, request.Team);
                
                return Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning team for match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while assigning the team");
            }
        }

        [HttpPost("{matchId}/rate-player")]
        public async Task<IActionResult> RatePlayer(int matchId, [FromBody] RatePlayerRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var result = await _matchService.RatePlayerAsync(
                    matchId,
                    userId,
                    request.RatedUserId,
                    request.SkillRating,
                    request.SportsmanshipRating,
                    request.Comment
                );
                
                return Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rating player for match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while rating the player");
            }
        }

        [HttpGet("{matchId}/ratings")]
        public async Task<IActionResult> GetMatchRatings(int matchId)
        {
            try
            {
                var ratings = await _matchService.GetMatchRatingsAsync(matchId);
                
                var ratingDtos = _mapper.Map<IEnumerable<PlayerRatingDto>>(ratings);
                return Ok(ratingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings for match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while getting match ratings");
            }
        }

        [HttpGet("user/{userId}/ratings")]
        public async Task<IActionResult> GetUserRatings(int userId)
        {
            try
            {
                var ratings = await _matchService.GetUserRatingsAsync(userId);
                
                var ratingDtos = _mapper.Map<IEnumerable<PlayerRatingDto>>(ratings);
                return Ok(ratingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings for user {UserId}", userId);
                return StatusCode(500, "An error occurred while getting user ratings");
            }
        }

        [HttpGet("{matchId}/has-rated-all")]
        public async Task<IActionResult> HasRatedAllPlayers(int matchId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var result = await _matchService.HasUserRatedAllPlayersAsync(matchId, userId);
                
                return Ok(new { HasRatedAll = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user has rated all players for match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while checking ratings");
            }
        }

        [HttpPost("{matchId}/start")]
        public async Task<IActionResult> StartMatch(int matchId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value);
                var result = await _matchService.StartMatchAsync(matchId, userId);
                
                return Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting match {MatchId}", matchId);
                return StatusCode(500, "An error occurred while starting the match");
            }
        }
    }

    public class CreateMatchRequest
    {
        public int BookingId { get; set; }
        public int TeamSize { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? MinSkillLevel { get; set; }
        public int? MaxSkillLevel { get; set; }
    }

    public class InvitePlayerRequest
    {
        public int InvitedUserId { get; set; }
    }

    public class RespondInvitationRequest
    {
        public bool Accept { get; set; }
    }

    public class RespondJoinRequest
    {
        public int RequesterId { get; set; }
        public bool Approve { get; set; }
    }

    public class AssignTeamRequest
    {
        public int PlayerId { get; set; }
        public string Team { get; set; }
    }

    public class RatePlayerRequest
    {
        public int RatedUserId { get; set; }
        public int SkillRating { get; set; }
        public int SportsmanshipRating { get; set; }
        public string Comment { get; set; }
    }
} 
