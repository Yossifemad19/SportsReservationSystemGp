using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.Core.Entities
{
    public class PlayerProfile : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        
        // Skill Assessment
        [Range(1, 10)]
        public int SkillLevel { get; set; } // 1-10 scale
        
        [JsonIgnore]
        public string SportSpecificSkillsJson { get; set; }
        
        [NotMapped]
        public Dictionary<string, int> SportSpecificSkills
        {
            get => string.IsNullOrEmpty(SportSpecificSkillsJson) 
                ? new Dictionary<string, int>() 
                : JsonSerializer.Deserialize<Dictionary<string, int>>(SportSpecificSkillsJson);
            set => SportSpecificSkillsJson = JsonSerializer.Serialize(value);
        }
        
        // Preferences
        public string PreferredPlayingStyle { get; set; }
        
        [JsonIgnore]
        public string PreferredSportsJson { get; set; }
        
        [NotMapped]
        public List<string> PreferredSports
        {
            get => string.IsNullOrEmpty(PreferredSportsJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(PreferredSportsJson);
            set => PreferredSportsJson = JsonSerializer.Serialize(value);
        }
        
        [JsonIgnore]
        public string PreferredPlayingTimesJson { get; set; }
        
        [NotMapped]
        public List<string> PreferredPlayingTimes
        {
            get => string.IsNullOrEmpty(PreferredPlayingTimesJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(PreferredPlayingTimesJson);
            set => PreferredPlayingTimesJson = JsonSerializer.Serialize(value);
        }
        
        // Social Preferences
        public bool PrefersCompetitivePlay { get; set; }
        public bool PrefersCasualPlay { get; set; }
        public int PreferredTeamSize { get; set; }
        
        // Availability
        [JsonIgnore]
        public string WeeklyAvailabilityJson { get; set; }
        
        [NotMapped]
        public Dictionary<DayOfWeek, List<TimeSpan>> WeeklyAvailability
        {
            get => string.IsNullOrEmpty(WeeklyAvailabilityJson) 
                ? new Dictionary<DayOfWeek, List<TimeSpan>>() 
                : JsonSerializer.Deserialize<Dictionary<DayOfWeek, List<TimeSpan>>>(WeeklyAvailabilityJson);
            set => WeeklyAvailabilityJson = JsonSerializer.Serialize(value);
        }
        
        // Social Connections
        [JsonIgnore]
        public string FrequentPartnersJson { get; set; }
        
        [NotMapped]
        public List<string> FrequentPartners
        {
            get => string.IsNullOrEmpty(FrequentPartnersJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(FrequentPartnersJson);
            set => FrequentPartnersJson = JsonSerializer.Serialize(value);
        }
        
        [JsonIgnore]
        public string BlockedPlayersJson { get; set; }
        
        [NotMapped]
        public List<string> BlockedPlayers
        {
            get => string.IsNullOrEmpty(BlockedPlayersJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(BlockedPlayersJson);
            set => BlockedPlayersJson = JsonSerializer.Serialize(value);
        }
        
        // Performance Metrics
        public int MatchesPlayed { get; set; }
        public int MatchesWon { get; set; }
        
        [NotMapped]
        public double WinRate => MatchesPlayed > 0 ? (double)MatchesWon / MatchesPlayed : 0;
        
        // Last Updated
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
} 