﻿using AutoMapper;
using backend.Api.DTOs;
using backend.Api.Hubs;
using backend.Api.Services.Interfaces;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;
using Microsoft.AspNetCore.SignalR;

namespace backend.Api.Services;

public class FriendRequestService : IFriendRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHubContext<NotificationHub> _hubContext;

    public FriendRequestService(IUnitOfWork unitOfWork , IMapper mapper , IHubContext<NotificationHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _hubContext = hubContext;

    }

    public async Task<(bool Success, string Message)> SendFriendRequestAsync(int senderId, int receiverId)
    {
        if (senderId == receiverId)
            return (false, "You cannot send a friend request to yourself.");

        var existing = await _unitOfWork.Repository<FriendRequest>()
            .FindAsync(fr => (fr.SenderId == senderId && fr.ReceiverId == receiverId) ||
                             (fr.SenderId == receiverId && fr.ReceiverId == senderId));
        if (existing != null)
            return (false, "A friend request already exists between these users.");

        var request = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending
        };

        _unitOfWork.Repository<FriendRequest>().Add(request);
        await _unitOfWork.Complete();

        // 🔔 Notify receiver via SignalR
        await _hubContext.Clients.User(receiverId.ToString())
            .SendAsync("ReceiveNotification", new
            {
                type = "FriendRequestReceived",
                fromUserId = senderId
            });

        return (true, "Friend request sent.");
    }

    public async Task<(bool Success, string Message)> AcceptFriendRequestAsync(int requestId, int userId)
    {
        var request = await _unitOfWork.Repository<FriendRequest>().GetByIdAsync(requestId);
        if (request == null || request.ReceiverId != userId)
            return (false, "Friend request not found or not authorized.");
        if (request.Status != FriendRequestStatus.Pending)
            return (false, "Friend request is not pending.");

        request.Status = FriendRequestStatus.Accepted;
        _unitOfWork.Repository<FriendRequest>().Update(request);
        await _unitOfWork.Complete();

        // 🔔 Notify sender via SignalR
        await _hubContext.Clients.User(request.SenderId.ToString())
            .SendAsync("ReceiveNotification", new
            {
                type = "FriendRequestAccepted",
                fromUserId = request.ReceiverId
            });

        return (true, "Friend request accepted.");
    }

    public async Task<(bool Success, string Message)> RejectFriendRequestAsync(int requestId, int userId)
    {
        var request = await _unitOfWork.Repository<FriendRequest>().GetByIdAsync(requestId);
        if (request == null || request.ReceiverId != userId)
            return (false, "Friend request not found or not authorized.");
        if (request.Status != FriendRequestStatus.Pending)
            return (false, "Friend request is not pending.");

        request.Status = FriendRequestStatus.Rejected;
        _unitOfWork.Repository<FriendRequest>().Update(request);
        await _unitOfWork.Complete();

        // 🔔 Optional: Notify sender about rejection
        await _hubContext.Clients.User(request.SenderId.ToString())
            .SendAsync("ReceiveNotification", new
            {
                type = "FriendRequestRejected",
                fromUserId = request.ReceiverId
            });

        return (true, "Friend request rejected.");
    }

    public async Task<List<FriendRequestDto>> GetPendingRequestsAsync(int userId)
    {
        var spec = new FriendRequestSpec(userId, FriendRequestStatus.Pending);
        var requests = await _unitOfWork.Repository<FriendRequest>().GetAllWithSpecAsync(spec);
        return _mapper.Map<List<FriendRequestDto>>(requests);
    }

    public async Task<List<FriendRequestDto>> GetReceivedRequestsAsync(int userId)
    {
        var spec = new FriendRequestSpec(userId, FriendRequestStatus.Pending, sentByUser: false);
        var requests = await _unitOfWork.Repository<FriendRequest>().GetAllWithSpecAsync(spec);
        return _mapper.Map<List<FriendRequestDto>>(requests);
    }

    public async Task<List<FriendRequestDto>> GetSentRequestsAsync(int userId)
    {
        var spec = new FriendRequestSpec(userId, FriendRequestStatus.Pending, sentByUser: true);
        var requests = await _unitOfWork.Repository<FriendRequest>().GetAllWithSpecAsync(spec);
        return _mapper.Map<List<FriendRequestDto>>(requests);
    }

    public async Task<List<FriendRequestDto>> GetAcceptedFriendRequestsAsync(int userId)
    {
        var spec = new FriendRequestSpec(userId); // use the correct constructor
        var requests = await _unitOfWork.Repository<FriendRequest>().GetAllWithSpecAsync(spec);
        return _mapper.Map<List<FriendRequestDto>>(requests);
    }


    public async Task<bool> AreFriendsAsync(int userId1, int userId2)
    {
        var repo = _unitOfWork.Repository<FriendRequest>();
        var friendship = await repo.FindAsync(fr =>
            fr.Status == FriendRequestStatus.Accepted &&
            (
                (fr.SenderId == userId1 && fr.ReceiverId == userId2) ||
                (fr.SenderId == userId2 && fr.ReceiverId == userId1)
            ));

        return friendship != null;
    }




}

