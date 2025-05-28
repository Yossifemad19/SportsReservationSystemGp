using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace backend.Core.Entities
{
    public class MatchPlayer : BaseEntity
    {
        [Required]
        public int MatchId { get; set; }
        
        [JsonIgnore]
        public Match Match { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public User User { get; set; }
        
        [Required]
        public ParticipationStatus Status { get; set; } = ParticipationStatus.Invited;
        
        public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResponseAt { get; set; }
        public DateTime? CheckedInAt { get; set; }
        
        public string? Team { get; set; } // e.g., "A" or "B"
    }
    
    public enum ParticipationStatus
    {
        [EnumMember(Value = "Invited")]
        Invited,
        
        [EnumMember(Value = "Accepted")]
        Accepted,
        
        [EnumMember(Value = "Declined")]
        Declined,
        
        [EnumMember(Value = "Requested")]
        Requested,
        
        [EnumMember(Value = "Approved")]
        Approved,
        
        [EnumMember(Value = "Rejected")]
        Rejected,
        
        [EnumMember(Value = "CheckedIn")]
        CheckedIn
    }
} 
