using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Api.DTOs;
using backend.Api.Services;
using backend.Api.Services.Interfaces;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;
using Microsoft.Extensions.Logging;
using AutoMapper;
using backend.Api.Exceptions;

namespace backend.Infrastructure.Services
{
    public class MatchService : IMatchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Match> _matchRepository;
        private readonly IGenericRepository<MatchPlayer> _matchPlayerRepository;
        private readonly IGenericRepository<PlayerRating> _playerRatingRepository;
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<PlayerProfile> _playerProfileRepository;
        private readonly ILogger<MatchService> _logger;
        private readonly IFriendRequestService _friendRequestService;
        private readonly IMapper _mapper;

        public MatchService(
            IUnitOfWork unitOfWork,
            ILogger<MatchService> logger, IFriendRequestService friendRequestService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _matchRepository = _unitOfWork.Repository<Match>();
            _matchPlayerRepository = _unitOfWork.Repository<MatchPlayer>();
            _playerRatingRepository = _unitOfWork.Repository<PlayerRating>();
            _bookingRepository = _unitOfWork.Repository<Booking>();
            _playerProfileRepository = _unitOfWork.Repository<PlayerProfile>();
            _logger = logger;
            _friendRequestService = friendRequestService;
            _mapper = mapper;
        }

        // Match CRUD operations
        public async Task<Match> CreateMatchAsync(int creatorUserId, int bookingId, int sportId, int teamSize, string title, string description, int? minSkillLevel, int? maxSkillLevel)
        {
            try
            {
                // Validate booking exists and belongs to the creator
                var booking = await _bookingRepository.FindAsync(b => b.Id == bookingId && b.UserId == creatorUserId);


                // Check if booking already has a match
                var existingMatch = await _matchRepository.FindAsync(m => m.BookingId == bookingId);

                // Validate sport exists
                var sport = await _unitOfWork.Repository<Sport>().GetByIdAsync(sportId);
                
                if (booking == null)
                    throw new BadRequestException("Booking not found or does not belong to the user");

                if (existingMatch != null)
                    throw new BadRequestException("A match already exists for this booking");

                if (sport == null)
                    throw new BadRequestException("Sport not found");


                var match = new Match
                {
                    CreatorUserId = creatorUserId,
                    BookingId = bookingId,
                    SportId = sportId,
                    TeamSize = teamSize,
                    Title = title,
                    Description = description,
                    MinSkillLevel = minSkillLevel,
                    MaxSkillLevel = maxSkillLevel,
                    Status = MatchStatus.Open,
                    CreatedAt = DateTime.UtcNow
                };

                _matchRepository.Add(match);
                await _unitOfWork.Complete();

                // Add creator as first player
                var player = new MatchPlayer
                {
                    MatchId = match.Id,
                    UserId = creatorUserId,
                    Status = ParticipationStatus.CheckedIn, // Creator is automatically checked in
                    InvitedAt = DateTime.UtcNow,
                    ResponseAt = DateTime.UtcNow,
                    CheckedInAt = DateTime.UtcNow,
                    Team = "A" // Default team for creator
                };

                _matchPlayerRepository.Add(player);
                await _unitOfWork.Complete();

                return match;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating match for user {UserId} and booking {BookingId}", creatorUserId, bookingId);
                throw;
            }
        }

        public async Task<bool> LeaveMatchAsync(int matchId, int userId)
        {
            try
            {
                var match = await _matchRepository.GetByIdAsync(matchId);
                if (match == null)
                    throw new InvalidOperationException("Match not found");

                if (match.CreatorUserId == userId)
                    throw new InvalidOperationException("Match creator cannot leave their own match");

                var matchPlayer = await _matchPlayerRepository.FindAsync(mp => mp.MatchId == matchId && mp.UserId == userId);
                if (matchPlayer == null)
                    throw new InvalidOperationException("User is not part of the match");

                _matchPlayerRepository.Remove(matchPlayer);
                await _unitOfWork.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving match {MatchId} by user {UserId}", matchId, userId);
                throw;
            }
        }

        public async Task KickPlayerAsync(int matchId, int creatorUserId, int targetUserId)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                throw new InvalidOperationException("Match not found");

            if (match.CreatorUserId != creatorUserId)
                throw new UnauthorizedAccessException("Only the match creator can kick players");

            var player = await _matchPlayerRepository.FindAsync(mp => mp.MatchId == matchId && mp.UserId == targetUserId);
            if (player == null)
                throw new InvalidOperationException("Player not found in this match");

            player.Status = ParticipationStatus.Kicked;
            player.WasKicked = true;
            player.Team = null;

            _matchPlayerRepository.Update(player);
            await _unitOfWork.Complete();
        }







        public async Task<Match> GetMatchByIdAsync(int matchId)
        {
            try
            {
                var spec = new MatchWithDetailsSpecification(matchId);
                return await _matchRepository.GetByIdWithSpecAsync(spec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting match with ID {MatchId}", matchId);
                throw;
            }
        }

        public async Task<List<MatchDto>> GetMatchesByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting matches for user {UserId}", userId);
                var spec = new MatchWithDetailsSpecification(userId, true);
                _logger.LogInformation("Created specification with criteria: {Criteria}", spec.Criteria?.ToString());
                
                var matches = await _matchRepository.GetAllWithSpecAsync(spec);
                _logger.LogInformation("Found {Count} matches for user {UserId}", matches.Count(), userId);
                
                if (!matches.Any())
                {
                    _logger.LogWarning("No matches found for user {UserId}. Checking if any matches exist in database...", userId);
                    var allMatches = await _matchRepository.GetAllAsync();
                    _logger.LogWarning("Total matches in database: {Count}", allMatches.Count());
                    if (allMatches.Any())
                    {
                        _logger.LogWarning("Sample match creator IDs: {CreatorIds}", 
                            string.Join(", ", allMatches.Take(5).Select(m => m.CreatorUserId)));
                    }
                }
                
                var matchDtos = _mapper.Map<List<MatchDto>>(matches);
                _logger.LogInformation("Mapped {Count} matches to DTOs", matchDtos.Count);
                
                return matchDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting matches for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<MatchDto>> GetAvailableMatchesAsync(int userId, int? sportId = null)
        {
            try
            {
                var spec = new AvailableMatchesSpecification(userId, sportId);
                var matches = await _matchRepository.GetAllWithSpecAsync(spec);

                var result = matches.Select(m => new MatchDto
                {
                    Id = m.Id,
                    CreatorUserId = m.CreatorUserId,
                    BookingId = m.BookingId,
                    Date = m.Booking?.Date,
                    StartTime = m.Booking?.StartTime,
                    EndTime = m.Booking?.EndTime,
                    SportId = m.SportId,
                    SportName = m.Sport?.Name,
                    TeamSize = m.TeamSize,
                    Status = m.Status.ToString(),
                    Title = m.Title,
                    Description = m.Description,
                    MinSkillLevel = m.MinSkillLevel,
                    MaxSkillLevel = m.MaxSkillLevel,
                    CreatedAt = m.CreatedAt,
                    CompletedAt = m.CompletedAt
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available matches for user {UserId} with sport ID {SportId}", userId, sportId);
                throw;
            }
        }

        public async Task<List<Match>> GetCompletedMatchesAsync(int userId)
        {
            try
            {
                var spec = new CompletedMatchesByUserIdSpecification(userId);
                var matches = await _matchRepository.GetAllWithSpecAsync(spec);
                return matches.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed matches for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CancelMatchAsync(int matchId, int userId)
        {
            try
            {
                var match = await _matchRepository.GetByIdAsync(matchId);
                if (match == null)
                {
                    throw new InvalidOperationException("Match not found");
                }
                
                // Only creator can cancel the match
                if (match.CreatorUserId != userId)
                {
                    throw new InvalidOperationException("Only the match creator can cancel the match");
                }
                
                // Only open matches can be cancelled
                if (match.Status != MatchStatus.Open)
                {
                    throw new InvalidOperationException("Cannot cancel a match that is not open");
                }
                
                match.Status = MatchStatus.Cancelled;
                _matchRepository.Update(match);
                await _unitOfWork.Complete();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling match {MatchId} by user {UserId}", matchId, userId);
                throw;
            }
        }

        public async Task<bool> CompleteMatchAsync(int matchId)
        {
            try
            {
                var match = await _matchRepository.GetByIdAsync(matchId);
                if (match == null)
                {
                    throw new InvalidOperationException("Match not found");
                }
                
                // Only in-progress matches can be completed
                if (match.Status != MatchStatus.InProgress)
                {
                    throw new InvalidOperationException("Only in-progress matches can be completed");
                }
                
                match.Status = MatchStatus.Completed;
                match.CompletedAt = DateTime.UtcNow;
                _matchRepository.Update(match);
                await _unitOfWork.Complete();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing match {MatchId}", matchId);
                throw;
            }
        }

        // Player management
        public async Task<bool> InvitePlayerToMatchAsync(int matchId, int invitedUserId, int inviterUserId)
        {
            try
            {
                var match = await _matchRepository.GetByIdAsync(matchId);
                if (match == null)
                {
                    throw new InvalidOperationException("Match not found");
                }

                if (!await _friendRequestService.AreFriendsAsync(inviterUserId, invitedUserId))
                {
                    throw new InvalidOperationException("You can only invite friends to the match");
                }

                // Check if inviter is creator or player
                var inviterIsPlayer = await _matchPlayerRepository.FindAsync(mp => mp.MatchId == matchId && mp.UserId == inviterUserId) != null;
                if (match.CreatorUserId != inviterUserId && !inviterIsPlayer)
                {
                    throw new InvalidOperationException("Only the match creator or players can invite others");
                }

                // Check if match is open
                if (match.Status != MatchStatus.Open)
                {
                    throw new InvalidOperationException("Cannot invite players to a match that is not open");
                }

                // Check if player is already active in the match (Accepted, Approved, CheckedIn)
                var existingActivePlayer = await _matchPlayerRepository.FindAsync(mp =>
                    mp.MatchId == matchId &&
                    mp.UserId == invitedUserId &&
                    (mp.Status == ParticipationStatus.Accepted ||
                     mp.Status == ParticipationStatus.Approved ||
                     mp.Status == ParticipationStatus.CheckedIn));

                if (existingActivePlayer != null)
                {
                    throw new InvalidOperationException("Player is already in the match");
                }

                // Check if player has a pending invitation (Invited or Requested)
                var existingPendingInvitation = await _matchPlayerRepository.FindAsync(mp =>
                    mp.MatchId == matchId &&
                    mp.UserId == invitedUserId &&
                    (mp.Status == ParticipationStatus.Invited ||
                     mp.Status == ParticipationStatus.Requested));

                if (existingPendingInvitation != null)
                {
                    throw new InvalidOperationException("Player already has a pending invitation");
                }

                // Check if match is full (count only active players)
                var currentPlayerCount = (await _matchPlayerRepository.GetAllAsync())
                    .Count(mp => mp.MatchId == matchId &&
                                 (mp.Status == ParticipationStatus.Accepted ||
                                  mp.Status == ParticipationStatus.Approved ||
                                  mp.Status == ParticipationStatus.CheckedIn));

                if (currentPlayerCount >= match.TeamSize * 2)
                {
                    throw new InvalidOperationException("Match is full");
                }

                // Add player as invited
                var player = new MatchPlayer
                {
                    MatchId = matchId,
                    UserId = invitedUserId,
                    Status = ParticipationStatus.Invited,
                    InvitedAt = DateTime.UtcNow
                };

                _matchPlayerRepository.Add(player);
                await _unitOfWork.Complete();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting user {InvitedUserId} to match {MatchId} by user {InviterUserId}", invitedUserId, matchId, inviterUserId);
                throw;
            }
        }


        public async Task<bool> RespondToInvitationAsync(int matchId, int userId, bool accept)
        {
            try
            {
                var invitation = await _matchPlayerRepository.FindAsync(mp => mp.MatchId == matchId && mp.UserId == userId && mp.Status == ParticipationStatus.Invited);
                if (invitation == null)
                {
                    throw new InvalidOperationException("Invitation not found");
                }
                
                // Check if match is still open
                var match = await _matchRepository.GetByIdAsync(matchId);
                if (match.Status != MatchStatus.Open)
                {
                    throw new InvalidOperationException("Cannot respond to invitation for a match that is not open");
                }
                
                // Update invitation status
                invitation.Status = accept ? ParticipationStatus.Accepted : ParticipationStatus.Declined;
                invitation.ResponseAt = DateTime.UtcNow;
                _matchPlayerRepository.Update(invitation);
                await _unitOfWork.Complete();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to invitation for match {MatchId} by user {UserId} with response {Accept}", matchId, userId, accept);
                throw;
            }
        }

        public async Task<bool> JoinMatchAsync(int matchId, int userId, string team)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                throw new InvalidOperationException("Match not found");

            var existingPlayer = await _matchPlayerRepository.FindAsync(mp => mp.MatchId == matchId && mp.UserId == userId);

            if (existingPlayer != null)
            {
                if (existingPlayer.WasKicked || existingPlayer.Status == ParticipationStatus.Kicked)
                    throw new InvalidOperationException("You have been kicked from this match and cannot rejoin.");

                if (existingPlayer.Status == ParticipationStatus.Accepted)
                    throw new InvalidOperationException("You are already part of this match");

                existingPlayer.Status = ParticipationStatus.Accepted;
                await _unitOfWork.Complete();
                return true;
            }

            var wasKickedBefore = await _matchPlayerRepository
                .FindAsync(mp => mp.MatchId == matchId && mp.UserId == userId && mp.WasKicked) != null;
            if (wasKickedBefore)
                throw new InvalidOperationException("You have been kicked from this match and cannot rejoin.");

            if (match.Status != MatchStatus.Open)
                throw new InvalidOperationException("Cannot join a match that is not open");

            var allPlayers = await _matchPlayerRepository.GetAllAsync();

            var currentPlayerCount = allPlayers
                .Count(mp => mp.MatchId == matchId && mp.Status != ParticipationStatus.Declined && mp.Status != ParticipationStatus.Rejected && mp.Status != ParticipationStatus.Kicked);

            if (currentPlayerCount >= match.TeamSize * 2)
                throw new InvalidOperationException("Match is full");

            // Validate team parameter
            if (string.IsNullOrEmpty(team) || (team != "A" && team != "B"))
                throw new InvalidOperationException("Team must be either 'A' or 'B'");

            var teamAPlayers = allPlayers
                .Count(mp => mp.MatchId == matchId && mp.Team == "A" && mp.Status != ParticipationStatus.Declined && mp.Status != ParticipationStatus.Rejected && mp.Status != ParticipationStatus.Kicked);
            var teamBPlayers = allPlayers
                .Count(mp => mp.MatchId == matchId && mp.Team == "B" && mp.Status != ParticipationStatus.Declined && mp.Status != ParticipationStatus.Rejected && mp.Status != ParticipationStatus.Kicked);

            // Check if the requested team is full
            if ((team == "A" && teamAPlayers >= match.TeamSize) || (team == "B" && teamBPlayers >= match.TeamSize))
                throw new InvalidOperationException($"Team {team} is full");

            var player = new MatchPlayer
            {
                MatchId = matchId,
                UserId = userId,
                Status = ParticipationStatus.Accepted,
                InvitedAt = DateTime.UtcNow,
                Team = team
            };

            _matchPlayerRepository.Add(player);
            await _unitOfWork.Complete();

            return true;
        }





        public async Task<bool> CheckInPlayerAsync(int matchId, int userId)
        {
            try
            {
                // Find player
                var player = await _matchPlayerRepository.FindAsync(mp => mp.MatchId == matchId && mp.UserId == userId);
                if (player == null)
                {
                    throw new InvalidOperationException("Player not found in match");
                }
                
                // Check if player can check in (accepted or approved)
                if (player.Status != ParticipationStatus.Accepted && player.Status != ParticipationStatus.Approved)
                {
                    throw new InvalidOperationException("Cannot check in with current status");
                }
                
                // Update status
                player.Status = ParticipationStatus.CheckedIn;
                player.CheckedInAt = DateTime.UtcNow;
                _matchPlayerRepository.Update(player);
                await _unitOfWork.Complete();
                
                // Check if all players are checked in, and if so, update match status
                var match = await _matchRepository.GetByIdAsync(matchId);
                if (match.Status == MatchStatus.Open && await AllPlayersCheckedInAsync(matchId))
                {
                    match.Status = MatchStatus.InProgress;
                    _matchRepository.Update(match);
                    await _unitOfWork.Complete();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in player {UserId} for match {MatchId}", userId, matchId);
                throw;
            }
        }

        public async Task<List<MatchPlayer>> GetMatchPlayersAsync(int matchId)
        {
            try
            {
                var spec = new MatchPlayersSpecification(matchId);
                var players = await _matchPlayerRepository.GetAllWithSpecAsync(spec);
                return players.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting players for match {MatchId}", matchId);
                throw;
            }
        }

        // Team management
        public async Task<bool> AssignTeamAsync(int matchId, int playerId, string team)
        {
            try
            {
                var player = await _matchPlayerRepository.FindAsync(mp => mp.MatchId == matchId && mp.UserId == playerId);
                if (player == null)
                {
                    throw new InvalidOperationException("Player not found in match");
                }
                
                
                // Validate team assignment
                if (string.IsNullOrEmpty(team) || (team != "A" && team != "B"))
                {
                    throw new InvalidOperationException("Team must be either 'A' or 'B'");
                }
                
                // Update team
                player.Team = team;
                _matchPlayerRepository.Update(player);
                await _unitOfWork.Complete();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning team {Team} to player {playerId} for match {MatchId}", team, playerId, matchId);
                throw;
            }
        }

        // Ratings
        public async Task<bool> RatePlayerAsync(int matchId, int raterUserId, int ratedUserId, int skillRating, int sportsmanshipRating, string comment)
        {
            try
            {
                // Check if match is completed
                var match = await _matchRepository.GetByIdAsync(matchId);
                if (match == null)
                {
                    throw new InvalidOperationException("Match not found");
                }
                
                if (match.Status != MatchStatus.Completed)
                {
                    throw new InvalidOperationException("Cannot rate players for a match that is not completed");
                }
                
                // Check if both users participated in the match
                var rater = await _matchPlayerRepository.FindAsync(mp => mp.MatchId == matchId && mp.UserId == raterUserId && mp.Status == ParticipationStatus.CheckedIn);
                var rated = await _matchPlayerRepository.FindAsync(mp => mp.MatchId == matchId && mp.UserId == ratedUserId && mp.Status == ParticipationStatus.CheckedIn);
                
                if (rater == null || rated == null)
                {
                    throw new InvalidOperationException("Both rater and rated user must have participated in the match");
                }
                
                // Check if user is rating themselves
                if (raterUserId == ratedUserId)
                {
                    throw new InvalidOperationException("Cannot rate yourself");
                }
                
                // Check if already rated
                var existingRating = await _playerRatingRepository.FindAsync(pr => pr.MatchId == matchId && pr.RaterUserId == raterUserId && pr.RatedUserId == ratedUserId);
                if (existingRating != null)
                {
                    throw new InvalidOperationException("You have already rated this player for this match");
                }
                
                // Create rating
                var rating = new PlayerRating
                {
                    MatchId = matchId,
                    RaterUserId = raterUserId,
                    RatedUserId = ratedUserId,
                    SkillRating = skillRating,
                    SportsmanshipRating = sportsmanshipRating,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                };
                
                _playerRatingRepository.Add(rating);
                await _unitOfWork.Complete();
                
                // Update player profile with new skill rating
                var ratedProfile = await _playerProfileRepository.FindAsync(p => p.UserId == ratedUserId);
                if (ratedProfile != null)
                {
                    // Update matches played and won
                    ratedProfile.MatchesPlayed += 1;
                    
                    // Update skill based on all ratings
                    var allRatings = await _playerRatingRepository.GetAllAsync();
                    var userRatings = allRatings.Where(r => r.RatedUserId == ratedUserId).ToList();
                    
                    if (userRatings.Any())
                    {
                        // Calculate new skill level (average of all ratings)
                        var averageSkill = userRatings.Average(r => r.SkillRating);
                        // Scale from 1-5 to 1-10
                        ratedProfile.SkillLevel = (int)Math.Round(averageSkill * 2);
                        // Ensure within bounds
                        ratedProfile.SkillLevel = Math.Min(10, Math.Max(1, ratedProfile.SkillLevel));
                    }
                    
                    _playerProfileRepository.Update(ratedProfile);
                    await _unitOfWork.Complete();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rating player {RatedUserId} by user {RaterUserId} for match {MatchId}", ratedUserId, raterUserId, matchId);
                throw;
            }
        }

        public async Task<List<PlayerRating>> GetMatchRatingsAsync(int matchId)
        {
            try
            {
                // Create a specification that includes the related users
                var spec = new PlayerRatingsWithUsersSpecification(matchId);
                var ratings = await _playerRatingRepository.GetAllWithSpecAsync(spec);
                return ratings.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings for match {MatchId}", matchId);
                throw;
            }
        }

        public async Task<List<PlayerRating>> GetUserRatingsAsync(int userId)
        {
            try
            {
                var allRatings = await _playerRatingRepository.GetAllAsync();
                return allRatings.Where(pr => pr.RatedUserId == userId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> HasUserRatedAllPlayersAsync(int matchId, int userId)
        {
            try
            {
                // Get all players in the match except the user
                var allPlayers = (await _matchPlayerRepository.GetAllAsync())
                    .Where(mp => mp.MatchId == matchId && mp.Status == ParticipationStatus.CheckedIn && mp.UserId != userId)
                    .ToList();
                
                if (!allPlayers.Any())
                {
                    return true; // No other players to rate
                }
                
                // Get all ratings by the user for this match
                var allRatings = await _playerRatingRepository.GetAllAsync();
                var userRatings = allRatings
                    .Where(pr => pr.MatchId == matchId && pr.RaterUserId == userId)
                    .ToList();
                
                // Get the user IDs that the user has rated
                var ratedUserIds = userRatings.Select(pr => pr.RatedUserId).ToList();
                
                // Check if all players have been rated
                return allPlayers.All(p => ratedUserIds.Contains(p.UserId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} has rated all players for match {MatchId}", userId, matchId);
                throw;
            }
        }

        // Match status management
        public async Task<bool> CanStartMatchAsync(int matchId)
        {
            try
            {
                var match = await _matchRepository.GetByIdAsync(matchId);
                if (match == null)
                {
                    throw new InvalidOperationException("Match not found");
                }
                
                // Match must be open
                if (match.Status != MatchStatus.Open)
                {
                    return false;
                }
                
                // Check if minimum number of players are checked in
                var checkedInPlayers = (await _matchPlayerRepository.GetAllAsync())
                    .Count(mp => mp.MatchId == matchId && mp.Status == ParticipationStatus.CheckedIn);
                
                // Need at least 2 players per team
                return checkedInPlayers >= 2 * 2;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if match {MatchId} can start", matchId);
                throw;
            }
        }

        public async Task<bool> AllPlayersCheckedInAsync(int matchId)
        {
            try
            {
                // Get all accepted/approved players
                var allPlayers = (await _matchPlayerRepository.GetAllAsync())
                    .Where(mp => mp.MatchId == matchId && (mp.Status == ParticipationStatus.Accepted || mp.Status == ParticipationStatus.Approved))
                    .ToList();
                
                // Get all checked-in players
                var checkedInPlayers = (await _matchPlayerRepository.GetAllAsync())
                    .Where(mp => mp.MatchId == matchId && mp.Status == ParticipationStatus.CheckedIn)
                    .ToList();
                
                // Return true if all accepted/approved players are checked in
                return allPlayers.Count == checkedInPlayers.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if all players are checked in for match {MatchId}", matchId);
                throw;
            }
        }

        public async Task<bool> StartMatchAsync(int matchId, int userId)
        {
            try
            {
                var match = await _matchRepository.GetByIdAsync(matchId);
                if (match == null)
                {
                    throw new InvalidOperationException("Match not found");
                }
                
                // Check if user is creator
                if (match.CreatorUserId != userId)
                {
                    throw new InvalidOperationException("Only the match creator can start the match");
                }
                
                // Check if match can start
                if (!await CanStartMatchAsync(matchId))
                {
                    throw new InvalidOperationException("Cannot start match at this time");
                }
                
                // Update match status
                match.Status = MatchStatus.InProgress;
                _matchRepository.Update(match);
                await _unitOfWork.Complete();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting match {MatchId} by user {UserId}", matchId, userId);
                throw;
            }
        }
    }
} 
