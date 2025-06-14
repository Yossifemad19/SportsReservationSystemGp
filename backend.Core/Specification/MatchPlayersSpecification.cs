using backend.Core.Entities;
using System;

namespace backend.Core.Specification;

public class MatchPlayersSpecification : BaseSpecification<MatchPlayer>
{
    // Constructor for filtering by matchId, includes User  
    public MatchPlayersSpecification(int matchId) : base(mp => mp.MatchId == matchId)
    {
        AddInclude(mp => mp.User); // Use strongly typed include if supported  
    }

    // Constructor for filtering by userId and status (Invited)  
    public MatchPlayersSpecification(int userId, ParticipationStatus status)
        : base(mp => mp.UserId == userId && mp.Status == status)
    {
        AddInclude(mp => mp.Match);
        AddInclude(mp => mp.Match.Sport); // Fixed: Use strongly typed lambda expression instead of string  
        AddInclude(mp => mp.InvitedByUser);  
        AddInclude(mp => mp.InvitedByUser);
    }
}
