using backend.Core.Entities;
using System;

namespace backend.Core.Specification
{
    public class PlayerRatingsWithUsersSpecification : BaseSpecification<PlayerRating>
    {
        public PlayerRatingsWithUsersSpecification(int matchId) 
            : base(pr => pr.MatchId == matchId)
        {
            AddInclude(pr => pr.RaterUser);
            AddInclude(pr => pr.RatedUser);
        }
    }
}