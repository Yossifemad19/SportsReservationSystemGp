using backend.Api.DTOs;
using backend.Core.Entities;

namespace backend.Api.Services.Interfaces;

public interface IFriendRequestService
{
    Task<(bool Success, string Message)> SendFriendRequestAsync(int senderId, int receiverId);
    Task<(bool Success, string Message)> AcceptFriendRequestAsync(int requestId, int userId);
    Task<(bool Success, string Message)> RejectFriendRequestAsync(int requestId, int userId);
    public Task<List<FriendRequestDto>> GetPendingRequestsAsync(int userId);
    Task<List<FriendRequestDto>> GetSentRequestsAsync(int userId);
    Task<List<FriendRequestDto>> GetReceivedRequestsAsync(int userId);
    Task<List<FriendRequestDto>> GetAcceptedFriendRequestsAsync(int userId);

}
