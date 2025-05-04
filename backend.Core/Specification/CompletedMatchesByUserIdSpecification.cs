using backend.Core.Entities;
using System;
using System.Linq;

namespace backend.Core.Specification
{
    public class CompletedMatchesByUserIdSpecification : BaseSpecification<Match>
    {
        public CompletedMatchesByUserIdSpecification(int userId) : base()
        {
            // Match is completed and user is a player
            Criteria = m => m.Status == MatchStatus.Completed && 
                           m.Players.Any(mp => mp.UserId == userId);
            
            AddInclude(m => m.Players);
            AddInclude(m => m.Booking);
            AddInclude(m => m.Ratings);
        }
    }
}