using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend.Core.Specification;

using System;
using System.Linq.Expressions;
using backend.Core.Entities;
using backend.Core.Specification;

public class FriendRequestSpec : BaseSpecification<FriendRequest>
{
    
    public FriendRequestSpec(int userId, FriendRequestStatus status, bool sentByUser = false)
        : base(fr =>
            (sentByUser ? fr.SenderId == userId : fr.ReceiverId == userId) &&
            fr.Status == status)
    {
        AddInclude(fr => fr.Sender);
        AddInclude(fr => fr.Receiver);

    }

    
    public FriendRequestSpec(int userId)
        : base(fr => (fr.SenderId == userId || fr.ReceiverId == userId) &&
                     fr.Status == FriendRequestStatus.Accepted)
    {
        AddInclude(fr => fr.Sender);
        AddInclude(fr => fr.Receiver);

    }
}


