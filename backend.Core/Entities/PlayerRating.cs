using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Core.Entities
{
    public class PlayerRating : BaseEntity
    {
        [Required]
        public int MatchId { get; set; }
        
        [JsonIgnore]
        public Match Match { get; set; }
        
        [Required]
        public int RaterUserId { get; set; }
        
        [JsonIgnore]
        public User RaterUser { get; set; }
        
        [Required]
        public int RatedUserId { get; set; }
        
        [JsonIgnore]
        public User RatedUser { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int SkillRating { get; set; } // 1-5 scale
        
        [Required]
        [Range(1, 5)]
        public int SportsmanshipRating { get; set; } // 1-5 scale
        
        [StringLength(500)]
        public string Comment { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 
