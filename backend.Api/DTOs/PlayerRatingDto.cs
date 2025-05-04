using System;

namespace backend.Api.DTOs
{
    public class PlayerRatingDto
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int RaterUserId { get; set; }
        public string RaterUserName { get; set; }
        public int RatedUserId { get; set; }
        public string RatedUserName { get; set; }
        public int SkillRating { get; set; }
        public int SportsmanshipRating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}