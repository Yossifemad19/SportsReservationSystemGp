using backend.Core.Entities;
using System;

namespace backend.Core.Specification;

public class MatchPlayersSpecification : BaseSpecification<MatchPlayer>
{
    
    public MatchPlayersSpecification(int matchId)
        : base(mp => mp.MatchId == matchId)
    {
        AddInclude(mp => mp.User);
    }

    
    public MatchPlayersSpecification(int userId, ParticipationStatus status)
        : base(mp => mp.UserId == userId && mp.Status == status)
    {
        AddInclude(mp => mp.Match);
        AddInclude(mp => mp.Match.Sport);
        AddInclude(mp => mp.InvitedByUser);
        AddInclude(mp => mp.Match.Booking);
        AddInclude(mp => mp.Match.Booking.Court);
        AddInclude(mp => mp.Match.Booking.Court.Facility);
        AddInclude(mp => mp.Match.Booking.Court.Facility.Address);
    }
}

