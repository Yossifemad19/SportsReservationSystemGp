using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend.Core.Entities;

public class FriendRequest : BaseEntity
{
    public int SenderId { get; set; }
    public User Sender { get; set; }
    public int ReceiverId { get; set; }
    public User Receiver { get; set; }
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum FriendRequestStatus
{
    Pending,
    Accepted,
    Rejected
}

