namespace backend.Api.DTOs;

public class FriendRequestDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderUserName { get; set; }
    public int ReceiverId { get; set; }
    public string ReceiverUserName { get; set; }
    public string Status { get; set; }
}

