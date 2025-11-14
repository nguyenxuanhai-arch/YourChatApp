using System;

namespace YourChatApp.Shared.Models
{
    [Serializable]
    public class Friend
    {
        public int FriendshipId { get; set; }
        public int UserId { get; set; }
        public int FriendUserId { get; set; }
        public FriendshipStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public Friend() { }

        public Friend(int userId, int friendUserId)
        {
            UserId = userId;
            FriendUserId = friendUserId;
            Status = FriendshipStatus.Pending;
            CreatedAt = DateTime.Now;
        }
    }

    public enum FriendshipStatus
    {
        Pending = 0,
        Accepted = 1,
        Blocked = 2
    }
}
