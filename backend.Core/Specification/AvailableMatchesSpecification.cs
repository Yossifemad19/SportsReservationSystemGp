using backend.Core.Entities;
using System;
using System.Linq;

namespace backend.Core.Specification
{
    public class AvailableMatchesSpecification : BaseSpecification<Match>
    {
        public AvailableMatchesSpecification(int userId, int? sportId = null) : base()
        {
            // Match is open and not created by the user
            Criteria = m => m.Status == MatchStatus.Open && 
                           m.CreatorUserId != userId &&
                           (!sportId.HasValue || m.SportId == sportId.Value);
            
            // Include related data
            AddInclude(m => m.Players);
            AddInclude(m => m.Booking);
            AddInclude(m => m.Sport);
            
            // Order by creation date
            OrderByDescending = m => m.CreatedAt;
        }
    }
}