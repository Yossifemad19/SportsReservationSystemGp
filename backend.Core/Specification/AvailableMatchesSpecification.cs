using backend.Core.Entities;
using System;
using System.Linq;

namespace backend.Core.Specification
{
    public class AvailableMatchesSpecification : BaseSpecification<Match>
    {
        public AvailableMatchesSpecification(int userId, string sportType = null) : base()
        {
            // Base criteria: match is open
            Criteria = m => m.Status == MatchStatus.Open;
            
            // Add sport type filter if provided
            if (!string.IsNullOrEmpty(sportType))
            {
                // Use the And method directly
                Criteria = And(Criteria, m => m.SportType == sportType);
            }
            
            // Exclude matches where user is already a player
            Criteria = And(Criteria, m => !m.Players.Any(mp => mp.UserId == userId));
            
            // Include only public matches or private matches where user is invited
            Criteria = And(Criteria, m => !m.IsPrivate || 
                                          m.Players.Any(mp => mp.UserId == userId && mp.Status == ParticipationStatus.Invited));
            
            AddInclude(m => m.Players);
            AddInclude(m => m.Booking);
        }
    }
}