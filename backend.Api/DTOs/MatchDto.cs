using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace backend.Api.DTOs
{
    public class MatchDto
    {
        public int Id { get; set; }
        public int CreatorUserId { get; set; }
        public int BookingId { get; set; }
        public string SportType { get; set; }
        public int TeamSize { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? MinSkillLevel { get; set; }
        public int? MaxSkillLevel { get; set; }
        public bool IsPrivate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        // Include simplified player information
        public List<MatchPlayerDto> Players { get; set; }
    }
    
    public class MatchPlayerDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public string Team { get; set; }
        public DateTime? InvitedAt { get; set; }
        public DateTime? ResponseAt { get; set; }
        public DateTime? CheckedInAt { get; set; }
    }
}