using backend.Core.Entities;
using System;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace backend.Core.Specification
{
    public class MatchesByUserIdSpecification : BaseSpecification<Match>
    {
        public MatchesByUserIdSpecification(int userId) : base()
        {
            AddInclude(m => m.Players);

            // Using string-based include for nested navigation  
            AddIncludeString("Players.User");

            // Filter matches where the user is a player  
            Criteria = m => m.Players.Any(mp => mp.UserId == userId);

            
            AddInclude(m => m.Booking);
        }
    }
}