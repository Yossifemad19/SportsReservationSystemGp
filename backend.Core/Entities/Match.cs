using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.Core.Entities
{
    public class Match : BaseEntity
    {
        [Required]
        public int CreatorUserId { get; set; }
        
        [Required]
        public int BookingId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Booking Booking { get; set; }
        
        [Required]
        public string SportType { get; set; }
        
        [Required]
        public int TeamSize { get; set; }
        
        [Required]
        public MatchStatus Status { get; set; }
        
        public string Title { get; set; }
        public string Description { get; set; }
        
        public int? MinSkillLevel { get; set; }
        public int? MaxSkillLevel { get; set; }
        
        public bool IsPrivate { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<MatchPlayer> Players { get; set; } = new List<MatchPlayer>();
        public virtual ICollection<PlayerRating> Ratings { get; set; } = new List<PlayerRating>();
    }
    
    public enum MatchStatus
    {
        [EnumMember(Value = "Open")]
        Open,
        
        [EnumMember(Value = "InProgress")]
        InProgress,
        
        [EnumMember(Value = "Completed")]
        Completed,
        
        [EnumMember(Value = "Cancelled")]
        Cancelled
    }
} 
