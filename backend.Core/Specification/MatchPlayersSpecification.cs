using backend.Core.Entities;
using System;

namespace backend.Core.Specification
{
    public class MatchPlayersSpecification : BaseSpecification<MatchPlayer>
    {
        public MatchPlayersSpecification(int matchId) : base(mp => mp.MatchId == matchId)
        {
            // Include related user data
            AddIncludeString("User");
        }
    }
}