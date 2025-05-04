using backend.Core.Entities;
using System;

namespace backend.Core.Specification
{
    public class MatchWithDetailsSpecification : BaseSpecification<Match>
    {
        public MatchWithDetailsSpecification(int matchId) : base(m => m.Id == matchId)
        {
            AddInclude(m => m.Players);
            AddInclude(m => m.Ratings);
            AddInclude(m => m.Booking);
        }
    }
}