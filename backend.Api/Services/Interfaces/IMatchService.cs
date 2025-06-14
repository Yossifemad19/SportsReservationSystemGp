using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Api.DTOs;
using backend.Core.Entities;

namespace backend.Core.Interfaces
{
    public interface IMatchService
    {
        // Match CRUD operations
        Task<Match> CreateMatchAsync(int creatorUserId, int bookingId, int sportId, int teamSize, string title, string description, int? minSkillLevel, int? maxSkillLevel);
        Task<Match> GetMatchByIdAsync(int matchId);
        Task<List<MatchDto>> GetMatchesByUserIdAsync(int userId);
        Task<List<MatchDto>> GetAvailableMatchesAsync(int userId, int? sportId = null);
        Task<List<Match>> GetCompletedMatchesAsync(int userId);
        Task<bool> CancelMatchAsync(int matchId, int userId);
        Task<bool> CompleteMatchAsync(int matchId);
        
        // Player management
        Task<bool> InvitePlayerToMatchAsync(int matchId, int invitedUserId, int inviterUserId);
        Task<bool> RespondToInvitationAsync(int matchId, int userId, bool accept);
        //Task<bool> RequestToJoinMatchAsync(int matchId, int userId);
        //Task<bool> RespondToJoinRequestAsync(int matchId, int requesterId, int responderId, bool approve);
        public Task<bool> JoinMatchAsync(int matchId, int userId,string team);
        Task<bool> CheckInPlayerAsync(int matchId, int userId);
        Task<List<MatchPlayer>> GetMatchPlayersAsync(int matchId);
        
        // Team management
        Task<bool> AssignTeamAsync(int matchId, int userId, string team);
        
        // Ratings
        Task<bool> RatePlayerAsync(int matchId, int raterUserId, int ratedUserId, int skillRating, int sportsmanshipRating, string comment);
        Task<List<PlayerRating>> GetMatchRatingsAsync(int matchId);
        Task<List<PlayerRating>> GetUserRatingsAsync(int userId);
        Task<bool> HasUserRatedAllPlayersAsync(int matchId, int userId);
        
        // Match status management
        Task<bool> CanStartMatchAsync(int matchId);
        Task<bool> AllPlayersCheckedInAsync(int matchId);
        Task<bool> StartMatchAsync(int matchId, int userId);
        public Task KickPlayerAsync(int matchId, int creatorUserId, int targetUserId);
        public Task<bool> LeaveMatchAsync(int matchId, int userId);
        public Task<List<MatchInvitationDto>> GetUserInvitationsAsync(int userId);
    }
} 