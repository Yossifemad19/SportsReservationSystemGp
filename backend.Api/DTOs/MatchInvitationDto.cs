using System;

namespace backend.Api.DTOs;

public class MatchInvitationDto
{
    public int MatchId { get; set; }
    public string MatchTitle { get; set; }
    public int SportId { get; set; }
    public string SportName { get; set; }

    public int InviterId { get; set; }
    public string InviterName { get; set; }
}