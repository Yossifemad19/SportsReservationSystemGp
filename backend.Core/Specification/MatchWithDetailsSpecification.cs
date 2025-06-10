using backend.Core.Entities;
using System;

namespace backend.Core.Specification
{
    public class MatchWithDetailsSpecification : BaseSpecification<Match>
    {
        public MatchWithDetailsSpecification(int matchId) : base(m => m.Id == matchId)
        {
            AddInclude(m => m.Players);
            AddIncludeString("Players.User");
            AddInclude(m => m.Ratings);
            AddInclude(m => m.Booking);
            AddInclude(m => m.Sport);
        }

        public MatchWithDetailsSpecification(int creatorUserId, bool isCreator = true) : base()
        {
            Criteria = m => m.CreatorUserId == creatorUserId;
            AddInclude(m => m.Players);
            AddIncludeString("Players.User");
            AddInclude(m => m.Ratings);
            AddInclude(m => m.Booking);
            AddInclude(m => m.Sport);
        }
    }
}